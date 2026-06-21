using Application.Interfaces.Services;

using Infrastructure.Interfaces;
using Infrastructure.Payments;

using Microsoft.Extensions.Options;

using NLog;

namespace Application.Services;

using Domain.Dto.Orders;
using Domain.Dto.Payments;
using Domain.Entities.AddressAndPayment;
using Domain.Entities.Catalogue;
using Domain.Entities.OrdersAndSubscriptions;

using Tools;

/// <summary>
/// Implémentation du service d'initialisation de paiement.
/// Recalcule les prix par palier côté serveur (jamais de confiance au montant client),
/// crée la commande + les abonnements en statut Pending, puis appelle la passerelle de paiement.
/// La confirmation (passage en Paid/Active + facture + vidage panier) est faite par le webhook.
/// </summary>
public class CheckoutService : ICheckoutService
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    private const decimal TaxRate = 0.20m;

    private readonly ICartRepository _cartRepository;
    private readonly IUserRepository _userRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IPaymentService _paymentService;
    private readonly PaymentOptions _paymentOptions;

    public CheckoutService(
        ICartRepository cartRepository,
        IUserRepository userRepository,
        IOrderRepository orderRepository,
        ISubscriptionRepository subscriptionRepository,
        IPaymentService paymentService,
        IOptions<PaymentOptions> paymentOptions)
    {
        _cartRepository         = cartRepository;
        _userRepository         = userRepository;
        _orderRepository        = orderRepository;
        _subscriptionRepository = subscriptionRepository;
        _paymentService         = paymentService;
        _paymentOptions         = paymentOptions.Value;
    }

    /// <inheritdoc />
    public async Task<PaymentInitResultDto> InitSubscriptionPaymentAsync(int userId, CreateOrderAddressDto address)
    {
        var user = await _userRepository.GetByIdAsync(userId)
                   ?? throw new KeyNotFoundException("Utilisateur introuvable.");

        var cartItems = (await _cartRepository.GetCartItemsAsync(userId)).ToList();
        if (cartItems.Count == 0)
            throw new InvalidOperationException("Le panier est vide.");

        var orderItems   = new List<OrderItem>();
        var paymentLines = new List<PaymentLineDto>();
        decimal total    = 0m;

        foreach (var item in cartItems)
        {
            if (item.QuantityUsers == 0 && item.QuantityDevices == 0)
                continue;

            var plan = await _cartRepository.GetPricingPlanWithTiersAsync(item.PricingPlanId)
                       ?? throw new KeyNotFoundException($"Plan tarifaire {item.PricingPlanId} introuvable.");

            var unitPriceUsers   = FindTier(plan.PricingTiers, BillingUnit.User,   item.QuantityUsers)?.PricePerUnit   ?? 0m;
            var unitPriceDevices = FindTier(plan.PricingTiers, BillingUnit.Device, item.QuantityDevices)?.PricePerUnit ?? 0m;
            var lineHt           = unitPriceUsers * item.QuantityUsers + unitPriceDevices * item.QuantityDevices;
            var lineTtc          = lineHt + lineHt * TaxRate;
            total               += lineTtc;

            var productName = plan.Product.Translations.FirstOrDefault(t => t.Locale == LocaleLang.Fr)?.Name
                           ?? plan.Product.Translations.FirstOrDefault()?.Name
                           ?? plan.Product.Slug;

            orderItems.Add(new OrderItem
            {
                ProductId           = plan.ProductId,
                PricingPlanId       = plan.Id,
                ProductNameSnapshot = productName,
                PlanNameSnapshot    = plan.BillingPeriod.ToString().ToLowerInvariant(),
                QuantityUsers       = item.QuantityUsers,
                QuantityDevices     = item.QuantityDevices,
                UnitPriceUsers      = unitPriceUsers,
                UnitPriceDevices    = unitPriceDevices,
            });

            paymentLines.Add(new PaymentLineDto
            {
                ProductId     = plan.ProductId,
                PricingPlanId = plan.Id,
                ProductName   = productName,
                BillingPeriod = plan.BillingPeriod,
                Amount        = lineTtc,
                Quantity      = 1,
            });
        }

        if (orderItems.Count == 0)
            throw new InvalidOperationException("Aucune ligne facturable dans le panier.");

        // ── 1. Commande + adresse en statut Pending (sans vider le panier) ────
        var billingAddress = new Address
        {
            UserId       = userId,
            FirstName    = address.FirstName,
            LastName     = address.LastName,
            AddressLine1 = address.Line1,
            PostalCode   = address.PostalCode,
            City         = address.City,
            Country      = address.Country,
        };

        var order = new Order
        {
            UserId      = userId,
            Status      = OrderStatus.Pending,
            TotalAmount = total,
            Items       = orderItems,
        };

        order = await _orderRepository.CreatePendingOrderAsync(billingAddress, order);

        // ── 2. Création du paiement chez le fournisseur ───────────────────────
        var currency = string.IsNullOrWhiteSpace(_paymentOptions.Currency) ? "eur" : _paymentOptions.Currency;

        _logger.Info("Initialisation paiement — userId={UserId}, orderId={OrderId}, {Count} ligne(s)",
            userId, order.Id, paymentLines.Count);

        var result = await _paymentService.CreateSubscriptionPaymentAsync(user, new CreateSubscriptionPaymentRequestDto
        {
            OrderId  = order.Id,
            Lines    = paymentLines,
            Currency = currency,
        });

        // ── 3. Abonnements locaux en statut Pending (confirmés par le webhook) ─
        var now = DateTime.UtcNow;
        var subscriptions = result.Subscriptions.Select(s => new Subscription
        {
            UserId               = userId,
            ProductId            = s.ProductId,
            PricingPlanId        = s.PricingPlanId,
            StripeSubscriptionId = s.StripeSubscriptionId,
            Status               = SubscriptionStatus.Pending,
            CurrentPeriodStart   = now,
            CurrentPeriodEnd     = s.BillingPeriod == BillingPeriod.Yearly ? now.AddYears(1) : now.AddMonths(1),
            AutoRenew            = true,
        }).ToList();

        if (subscriptions.Count > 0)
            await _subscriptionRepository.AddRangeAsync(subscriptions);

        // ── 4. Lien du PaymentIntent "à vie" sur la commande ──────────────────
        if (!string.IsNullOrEmpty(result.LifetimePaymentIntentId))
        {
            order.StripePaymentIntentId = result.LifetimePaymentIntentId;
            await _orderRepository.UpdateOrderAsync(order);
        }

        result.OrderId = order.Id;
        return result;
    }

    private static PricingTier? FindTier(IEnumerable<PricingTier> tiers, BillingUnit unitType, int quantity)
    {
        if (quantity <= 0) return null;
        return tiers.FirstOrDefault(t =>
            t.unitType == unitType &&
            quantity >= t.minQuantity &&
            quantity <= t.maxQuantity);
    }
}
