namespace Infrastructure.Payments;

using Domain.Dto.Payments;
using Domain.Entities;

using Infrastructure.Interfaces;

using Microsoft.Extensions.Options;

using NLog;

using Stripe;

using Tools;

/// <summary>
/// Implémentation Stripe de la passerelle de paiement (active lorsque <c>Payments:Provider</c> vaut "Stripe").
/// Crée le client, puis une Subscription Stripe par périodicité de facturation (les prix sont fournis
/// en <c>price_data</c> inline) ; les lignes "à vie" donnent un PaymentIntent unique.
/// </summary>
public class StripePaymentService : IPaymentService
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    private readonly IUserRepository _userRepository;

    private readonly CustomerService _customers = new();
    private readonly ProductService _products = new();
    private readonly SubscriptionService _subscriptions = new();
    private readonly PaymentIntentService _paymentIntents = new();

    public StripePaymentService(IUserRepository userRepository, IOptions<StripeOptions> options)
    {
        _userRepository = userRepository;

        // Clé API globale du SDK Stripe.
        StripeConfiguration.ApiKey = options.Value.SecretKey;
        _logger.Debug("StripePaymentService initialisé (provider=Stripe).");
    }

    /// <inheritdoc />
    public async Task<string> EnsureCustomerAsync(User user)
    {
        if (!string.IsNullOrEmpty(user.StripeCustomerId))
            return user.StripeCustomerId;

        var customer = await _customers.CreateAsync(new CustomerCreateOptions
        {
            Email    = user.Email,
            Name     = $"{user.FirstName} {user.LastName}".Trim(),
            Metadata = new Dictionary<string, string> { ["userId"] = user.Id.ToString() },
        });

        user.StripeCustomerId = customer.Id;
        await _userRepository.UpdateAsync(user);

        _logger.Info("Client Stripe {CustomerId} créé pour l'utilisateur ID {UserId}", customer.Id, user.Id);
        return customer.Id;
    }

    /// <inheritdoc />
    public async Task<PaymentInitResultDto> CreateSubscriptionPaymentAsync(User user, CreateSubscriptionPaymentRequestDto request)
    {
        var customerId = await EnsureCustomerAsync(user);
        var currency   = string.IsNullOrWhiteSpace(request.Currency) ? "eur" : request.Currency.ToLowerInvariant();

        var subscriptions = new List<RecurringPaymentResultDto>();

        // Cache produit Stripe par nom : évite de recréer le même produit plusieurs fois dans la requête.
        var productCache = new Dictionary<string, string>();

        // ── Lignes récurrentes : une Subscription Stripe par ligne (1:1 avec la Subscription locale) ──
        foreach (var line in request.Lines.Where(l => l.BillingPeriod != BillingPeriod.Lifetime))
        {
            var interval  = line.BillingPeriod == BillingPeriod.Yearly ? "year" : "month";
            var productId = await EnsureProductAsync(line.ProductName, productCache);

            var options = new SubscriptionCreateOptions
            {
                Customer = customerId,
                Items =
                [
                    new SubscriptionItemOptions
                    {
                        PriceData = new SubscriptionItemPriceDataOptions
                        {
                            Currency   = currency,
                            Product    = productId,
                            UnitAmount = ToMinorUnits(line.Amount),
                            Recurring  = new SubscriptionItemPriceDataRecurringOptions { Interval = interval },
                        },
                        Quantity = Math.Max(1, line.Quantity),
                    },
                ],
                PaymentBehavior = "default_incomplete",
                PaymentSettings = new SubscriptionPaymentSettingsOptions
                {
                    SaveDefaultPaymentMethod = "on_subscription",
                    PaymentMethodTypes       = ["card"],
                },
                Metadata = new Dictionary<string, string>
                {
                    ["userId"]        = user.Id.ToString(),
                    ["orderId"]       = request.OrderId.ToString(),
                    ["pricingPlanId"] = line.PricingPlanId.ToString(),
                    ["productId"]     = line.ProductId.ToString(),
                },
            };
            // Récupère le client secret de la première facture pour confirmation côté front.
            options.AddExpand("latest_invoice.confirmation_secret");

            var subscription = await _subscriptions.CreateAsync(options);

            subscriptions.Add(new RecurringPaymentResultDto
            {
                ProductId            = line.ProductId,
                PricingPlanId        = line.PricingPlanId,
                BillingPeriod        = line.BillingPeriod,
                StripeSubscriptionId = subscription.Id,
                ClientSecret         = subscription.LatestInvoice?.ConfirmationSecret?.ClientSecret ?? string.Empty,
            });

            _logger.Info("Subscription Stripe {SubId} créée ({Interval}) pour la commande {OrderId}",
                subscription.Id, interval, request.OrderId);
        }

        // ── Lignes "à vie" : un PaymentIntent unique pour le total ────────────
        string? lifetimePaymentIntentId = null;
        string? lifetimeClientSecret    = null;

        var lifetimeAmount = request.Lines
            .Where(l => l.BillingPeriod == BillingPeriod.Lifetime)
            .Sum(l => l.Amount * Math.Max(1, l.Quantity));

        if (lifetimeAmount > 0m)
        {
            var paymentIntent = await _paymentIntents.CreateAsync(new PaymentIntentCreateOptions
            {
                Amount                  = ToMinorUnits(lifetimeAmount),
                Currency                = currency,
                Customer                = customerId,
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions { Enabled = true },
                Metadata = new Dictionary<string, string>
                {
                    ["userId"]  = user.Id.ToString(),
                    ["orderId"] = request.OrderId.ToString(),
                    ["type"]    = "lifetime",
                },
            });

            lifetimePaymentIntentId = paymentIntent.Id;
            lifetimeClientSecret    = paymentIntent.ClientSecret;

            _logger.Info("PaymentIntent à vie {PiId} créé pour la commande {OrderId}", paymentIntent.Id, request.OrderId);
        }

        return new PaymentInitResultDto
        {
            OrderId                 = request.OrderId,
            CustomerId              = customerId,
            Subscriptions           = subscriptions,
            LifetimePaymentIntentId = lifetimePaymentIntentId,
            LifetimeClientSecret    = lifetimeClientSecret,
        };
    }

    /// <summary>Crée (ou réutilise dans la requête) un produit Stripe pour le libellé donné.</summary>
    private async Task<string> EnsureProductAsync(string name, IDictionary<string, string> cache)
    {
        var key = string.IsNullOrWhiteSpace(name) ? "Cyna" : name;
        if (cache.TryGetValue(key, out var existingId))
            return existingId;

        var product = await _products.CreateAsync(new ProductCreateOptions { Name = key });
        cache[key] = product.Id;
        return product.Id;
    }

    /// <summary>Convertit un montant décimal en plus petite unité monétaire (centimes).</summary>
    private static long ToMinorUnits(decimal amount) => (long)Math.Round(amount * 100m, MidpointRounding.AwayFromZero);
}
