using Application.Interfaces.Services;

using Infrastructure.Interfaces;
using Infrastructure.Payments;

using Microsoft.Extensions.Options;

using NLog;

namespace Application.Services;

using Domain.Entities.OrdersAndSubscriptions;

using Tools;

/// <summary>
/// Traite les webhooks Stripe : c'est la source de vérité du paiement.
/// Confirme les commandes/abonnements créés en Pending lors de l'initialisation.
/// Les types Stripe sont pleinement qualifiés pour éviter la collision avec les entités du domaine.
/// </summary>
public class PaymentWebhookService : IPaymentWebhookService
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    private readonly IOrderRepository _orderRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly ICartRepository _cartRepository;
    private readonly StripeOptions _options;

    public PaymentWebhookService(
        IOrderRepository orderRepository,
        ISubscriptionRepository subscriptionRepository,
        ICartRepository cartRepository,
        IOptions<StripeOptions> options)
    {
        _orderRepository        = orderRepository;
        _subscriptionRepository = subscriptionRepository;
        _cartRepository         = cartRepository;
        _options                = options.Value;
    }

    /// <inheritdoc />
    public async Task HandleEventAsync(string json, string signatureHeader)
    {
        // throwOnApiVersionMismatch = false : on tolère un écart de version d'API entre le compte et le SDK.
        var stripeEvent = Stripe.EventUtility.ConstructEvent(json, signatureHeader, _options.WebhookSecret, 300, false);

        _logger.Info("Webhook Stripe reçu : {Type}", stripeEvent.Type);

        switch (stripeEvent.Type)
        {
            case "invoice.paid":
                await HandleInvoicePaidAsync(stripeEvent.Data.Object as Stripe.Invoice);
                break;
            case "invoice.payment_failed":
                await HandleInvoiceFailedAsync(stripeEvent.Data.Object as Stripe.Invoice);
                break;
            case "payment_intent.succeeded":
                await HandlePaymentIntentSucceededAsync(stripeEvent.Data.Object as Stripe.PaymentIntent);
                break;
            case "customer.subscription.updated":
                await SyncSubscriptionStatusAsync(stripeEvent.Data.Object as Stripe.Subscription);
                break;
            case "customer.subscription.deleted":
                await CancelSubscriptionAsync(stripeEvent.Data.Object as Stripe.Subscription);
                break;
            default:
                _logger.Debug("Événement Stripe ignoré : {Type}", stripeEvent.Type);
                break;
        }
    }

    // ── invoice.paid : abonnement payé → Subscription Active + Order Paid + facture ──
    private async Task HandleInvoicePaidAsync(Stripe.Invoice? invoice)
    {
        var details = invoice?.Parent?.SubscriptionDetails;
        if (details is null || string.IsNullOrEmpty(details.SubscriptionId))
            return;

        var subscription = await _subscriptionRepository.GetByStripeIdAsync(details.SubscriptionId);
        if (subscription is not null && subscription.Status != SubscriptionStatus.Active)
        {
            subscription.Status = SubscriptionStatus.Active;
            await _subscriptionRepository.UpdateAsync(subscription);
        }

        if (TryGetOrderId(details.Metadata, out var orderId))
            await MarkOrderPaidAsync(orderId, invoice!.Number, invoice.HostedInvoiceUrl ?? invoice.InvoicePdf);
    }

    // ── payment_intent.succeeded : paiement "à vie" → Order Paid + facture ──
    private async Task HandlePaymentIntentSucceededAsync(Stripe.PaymentIntent? paymentIntent)
    {
        // On ne traite ici que les paiements "à vie" ; les PI d'abonnement sont gérés via invoice.paid.
        if (paymentIntent?.Metadata is null
            || !paymentIntent.Metadata.TryGetValue("type", out var type) || type != "lifetime")
            return;

        if (TryGetOrderId(paymentIntent.Metadata, out var orderId))
            await MarkOrderPaidAsync(orderId, number: null, pdfUrl: null);
    }

    // ── invoice.payment_failed : paiement échoué → Order Failed ──
    private async Task HandleInvoiceFailedAsync(Stripe.Invoice? invoice)
    {
        var details = invoice?.Parent?.SubscriptionDetails;
        if (details is null || !TryGetOrderId(details.Metadata, out var orderId))
            return;

        var order = await _orderRepository.GetTrackedByIdAsync(orderId);
        if (order is not null && order.Status == OrderStatus.Pending)
        {
            order.Status = OrderStatus.Failed;
            await _orderRepository.UpdateOrderAsync(order);
            _logger.Info("Commande {OrderId} marquée Failed (paiement échoué)", orderId);
        }
    }

    private async Task SyncSubscriptionStatusAsync(Stripe.Subscription? stripeSubscription)
    {
        if (stripeSubscription is null) return;

        var subscription = await _subscriptionRepository.GetByStripeIdAsync(stripeSubscription.Id);
        if (subscription is null) return;

        subscription.Status = MapStatus(stripeSubscription.Status, subscription.Status);
        await _subscriptionRepository.UpdateAsync(subscription);
    }

    private async Task CancelSubscriptionAsync(Stripe.Subscription? stripeSubscription)
    {
        if (stripeSubscription is null) return;

        var subscription = await _subscriptionRepository.GetByStripeIdAsync(stripeSubscription.Id);
        if (subscription is null || subscription.Status == SubscriptionStatus.Cancelled) return;

        subscription.Status = SubscriptionStatus.Cancelled;
        await _subscriptionRepository.UpdateAsync(subscription);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task MarkOrderPaidAsync(int orderId, string? number, string? pdfUrl)
    {
        var order = await _orderRepository.GetTrackedByIdAsync(orderId);
        if (order is null) return;

        if (order.Status != OrderStatus.Paid)
        {
            order.Status = OrderStatus.Paid;
            await _orderRepository.UpdateOrderAsync(order);
            _logger.Info("Commande {OrderId} confirmée Paid", orderId);
        }

        // Idempotence : une seule facture par commande.
        if (!await _orderRepository.InvoiceExistsForOrderAsync(orderId))
        {
            await _orderRepository.AddInvoiceAsync(new Invoice
            {
                OrderId       = orderId,
                InvoiceNumber = !string.IsNullOrEmpty(number) ? number : $"CYNA-{orderId}-{DateTime.UtcNow:yyyyMMddHHmmss}",
                PdfUrl        = !string.IsNullOrEmpty(pdfUrl) ? pdfUrl : $"https://invoice.local/{orderId}",
                IssuedAt      = DateTime.UtcNow,
            });
        }

        // Le paiement est confirmé : on vide le panier (no-op s'il est déjà vide).
        await _cartRepository.ClearCartAsync(order.UserId);
    }

    private static bool TryGetOrderId(IDictionary<string, string>? metadata, out int orderId)
    {
        orderId = 0;
        return metadata is not null
            && metadata.TryGetValue("orderId", out var raw)
            && int.TryParse(raw, out orderId)
            && orderId > 0;
    }

    private static SubscriptionStatus MapStatus(string stripeStatus, SubscriptionStatus current) => stripeStatus switch
    {
        "active" or "trialing"            => SubscriptionStatus.Active,
        "canceled"                        => SubscriptionStatus.Cancelled,
        "past_due" or "unpaid"            => SubscriptionStatus.Suspended,
        "incomplete" or "incomplete_expired" => SubscriptionStatus.Pending,
        _                                 => current,
    };
}
