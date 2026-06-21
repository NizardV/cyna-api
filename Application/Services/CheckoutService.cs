using Application.Interfaces.Services;

using Infrastructure.Interfaces;
using Infrastructure.Payments;

using Microsoft.Extensions.Options;

using NLog;

namespace Application.Services;

using Domain.Dto.Payments;
using Domain.Entities.Catalogue;

using Tools;

/// <summary>
/// Implémentation du service d'initialisation de paiement.
/// Recalcule les prix par palier côté serveur (jamais de confiance au montant client),
/// construit les lignes facturables puis appelle la passerelle de paiement.
/// </summary>
public class CheckoutService : ICheckoutService
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    private const decimal TaxRate = 0.20m;

    private readonly ICartRepository _cartRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPaymentService _paymentService;
    private readonly PaymentOptions _paymentOptions;

    public CheckoutService(
        ICartRepository cartRepository,
        IUserRepository userRepository,
        IPaymentService paymentService,
        IOptions<PaymentOptions> paymentOptions)
    {
        _cartRepository = cartRepository;
        _userRepository = userRepository;
        _paymentService = paymentService;
        _paymentOptions = paymentOptions.Value;
    }

    /// <inheritdoc />
    public async Task<PaymentInitResultDto> InitSubscriptionPaymentAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId)
                   ?? throw new KeyNotFoundException("Utilisateur introuvable.");

        var cartItems = (await _cartRepository.GetCartItemsAsync(userId)).ToList();
        if (cartItems.Count == 0)
            throw new InvalidOperationException("Le panier est vide.");

        var lines = new List<PaymentLineDto>();

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

            var productName = plan.Product.Translations.FirstOrDefault(t => t.Locale == LocaleLang.Fr)?.Name
                           ?? plan.Product.Translations.FirstOrDefault()?.Name
                           ?? plan.Product.Slug;

            lines.Add(new PaymentLineDto
            {
                ProductId     = plan.ProductId,
                PricingPlanId = plan.Id,
                ProductName   = productName,
                BillingPeriod = plan.BillingPeriod,
                Amount        = lineTtc,
                Quantity      = 1,
            });
        }

        if (lines.Count == 0)
            throw new InvalidOperationException("Aucune ligne facturable dans le panier.");

        var currency = string.IsNullOrWhiteSpace(_paymentOptions.Currency) ? "eur" : _paymentOptions.Currency;

        _logger.Info("Initialisation paiement — userId={UserId}, {Count} ligne(s)", userId, lines.Count);

        return await _paymentService.CreateSubscriptionPaymentAsync(user, new CreateSubscriptionPaymentRequestDto
        {
            Lines    = lines,
            Currency = currency,
        });
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
