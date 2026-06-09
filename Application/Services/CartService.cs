using Application.Interfaces.Services;

using Infrastructure.Interfaces;

using NLog;

namespace Application.Services;

using Domain.Dto.Cart;
using Domain.Entities.Catalogue;

using Infrastructure.Entities;

public class CartService : ICartService
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    private const decimal TaxRate = 0.20m;

    private readonly ICartRepository _cartRepository;

    public CartService(ICartRepository cartRepository)
    {
        _cartRepository = cartRepository;
    }

    /// <inheritdoc />
    public async Task<CartResultDto> AddOrUpdateCartItemAsync(int userId, AddCartItemRequestDto dto)
    {
        if (dto.QuantityUsers == 0 && dto.QuantityDevices == 0)
            throw new ArgumentException("Au moins une quantité (utilisateurs ou appareils) doit être supérieure à zéro.");

        var plan = await _cartRepository.GetPricingPlanWithTiersAsync(dto.PricingPlanId)
                   ?? throw new KeyNotFoundException($"Plan tarifaire {dto.PricingPlanId} introuvable.");

        _logger.Info("Ajout au panier — userId={UserId}, planId={PlanId}", userId, dto.PricingPlanId);

        var tierUser   = FindTier(plan.PricingTiers, BillingUnit.User,   dto.QuantityUsers);
        var tierDevice = FindTier(plan.PricingTiers, BillingUnit.Device, dto.QuantityDevices);

        var unitPriceUsers   = tierUser?.PricePerUnit   ?? 0m;
        var unitPriceDevices = tierDevice?.PricePerUnit ?? 0m;
        var lineTotal        = unitPriceUsers * dto.QuantityUsers + unitPriceDevices * dto.QuantityDevices;

        var cartItem = await _cartRepository.UpsertCartItemAsync(
            userId, plan.ProductId, dto.PricingPlanId,
            dto.QuantityUsers, dto.QuantityDevices);

        var allItems  = await _cartRepository.GetCartItemsAsync(userId);
        var subtotal  = allItems.Sum(ci => CalculateLineTotal(ci.PricingPlan, ci.QuantityUsers, ci.QuantityDevices));
        var taxAmount = subtotal * TaxRate;

        var maxUsers   = plan.PricingTiers.Where(t => t.unitType == BillingUnit.User)  .Select(t => t.maxQuantity).DefaultIfEmpty(0).Max();
        var maxDevices = plan.PricingTiers.Where(t => t.unitType == BillingUnit.Device).Select(t => t.maxQuantity).DefaultIfEmpty(0).Max();

        var productName = plan.Product.Translations.FirstOrDefault(t => t.Locale == LocaleLang.Fr)?.Name
                       ?? plan.Product.Translations.FirstOrDefault()?.Name
                       ?? plan.Product.Slug;

        return new CartResultDto
        {
            CartId             = userId,
            StripeClientSecret = $"pi_mock_{Guid.NewGuid()}_secret_mock",
            Item = new CartItemResultDto
            {
                Id                  = cartItem.Id,
                PricingPlanId       = plan.Id,
                ProductName         = productName,
                BillingPeriod       = plan.BillingPeriod.ToString().ToLowerInvariant(),
                QuantityUsers       = dto.QuantityUsers,
                QuantityDevices     = dto.QuantityDevices,
                UnitPriceUsers      = unitPriceUsers,
                UnitPriceDevices    = unitPriceDevices,
                LineTotal           = lineTotal,
                MaxUsersCheckout    = maxUsers,
                MaxDevicesCheckout  = maxDevices,
                PricingTiers        = plan.PricingTiers.Select(t => new CartTierDto
                {
                    UnitType  = t.unitType.ToString().ToLowerInvariant(),
                    MinQty    = t.minQuantity,
                    MaxQty    = t.maxQuantity,
                    UnitPrice = t.PricePerUnit,
                }),
            },
            CartSummary = new CartSummaryDto
            {
                Subtotal  = subtotal,
                TaxAmount = taxAmount,
                Total     = subtotal + taxAmount,
            },
        };
    }


    private static PricingTier? FindTier(IEnumerable<PricingTier> tiers, BillingUnit unitType, int quantity)
    {
        if (quantity <= 0) return null;
        return tiers.FirstOrDefault(t =>
            t.unitType == unitType &&
            quantity >= t.minQuantity &&
            quantity <= t.maxQuantity);
    }

    private static decimal CalculateLineTotal(PricingPlan plan, int quantityUsers, int quantityDevices)
    {
        var tierUser   = FindTier(plan.PricingTiers, BillingUnit.User,   quantityUsers);
        var tierDevice = FindTier(plan.PricingTiers, BillingUnit.Device, quantityDevices);
        return (tierUser?.PricePerUnit ?? 0m) * quantityUsers
             + (tierDevice?.PricePerUnit ?? 0m) * quantityDevices;
    }
}
