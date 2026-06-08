using NLog;

namespace Application.Services;

using Domain.Dto.Orders;
using Domain.Dto.User;
using Domain.Entities.AddressAndPayment;
using Domain.Entities.Catalogue;
using Domain.Entities.OrdersAndSubscriptions;

using Infrastructure.Entities;
using Infrastructure.Interfaces;

using Interfaces;

/// <summary>
/// Service de gestion de l'historique des commandes utilisateur.
/// Mappe les entités de la base de données vers les DTOs de l'API.
/// </summary>
public class OrderService : IOrderService
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    private const decimal TaxRate = 0.20m;

    private readonly IOrderRepository _orderRepository;
    private readonly ICartRepository  _cartRepository;

    public OrderService(IOrderRepository orderRepository, ICartRepository cartRepository)
    {
        _orderRepository = orderRepository;
        _cartRepository  = cartRepository;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<OrderSummaryDto>> GetUserOrdersAsync(int userId)
    {
        _logger.Info("Récupération des commandes pour l'utilisateur ID {UserId}", userId);

        var orders = await _orderRepository.GetByUserIdAsync(userId);

        return orders.Select(o => new OrderSummaryDto
        {
            Id          = o.Id,
            Status      = o.Status.ToString(),
            TotalAmount = o.TotalAmount,
            CreatedAt   = o.CreatedAt,
            InvoiceUrl  = o.Invoices.FirstOrDefault()?.PdfUrl,
            Items       = o.Items.Select(i => new OrderItemDto
            {
                Id                  = i.Id,
                ProductNameSnapshot = i.ProductNameSnapshot,
                PlanNameSnapshot    = i.PlanNameSnapshot,
                QuantityUsers       = i.QuantityUsers,
                QuantityDevices     = i.QuantityDevices,
            }).ToList(),
        });
    }

    /// <inheritdoc />
    public async Task<CreateOrderResultDto> CreateOrderAsync(int userId, CreateOrderRequestDto dto)
    {
        _logger.Info("Création commande pour l'utilisateur ID {UserId}", userId);

        // ── 1. Validation + calcul des prix par item ──────────────────────
        var lines = new List<(CreateOrderItemDto Dto, PricingPlan Plan, decimal UnitPriceUsers, decimal UnitPriceDevices, decimal LineTotal)>();

        foreach (var item in dto.Items)
        {
            if (item.QuantityUsers == 0 && item.QuantityDevices == 0)
                throw new ArgumentException("Au moins une quantité doit être supérieure à zéro.");

            var plan = await _cartRepository.GetPricingPlanWithTiersAsync(item.PricingPlanId)
                       ?? throw new KeyNotFoundException($"Plan tarifaire {item.PricingPlanId} introuvable.");

            var maxUsers   = plan.PricingTiers.Where(t => t.unitType == BillingUnit.User)  .Select(t => t.maxQuantity).DefaultIfEmpty(0).Max();
            var maxDevices = plan.PricingTiers.Where(t => t.unitType == BillingUnit.Device).Select(t => t.maxQuantity).DefaultIfEmpty(0).Max();

            if (item.QuantityUsers > maxUsers || item.QuantityDevices > maxDevices)
                throw new InvalidOperationException("Quote required for one or more items.");

            var tierUser   = FindTier(plan.PricingTiers, BillingUnit.User,   item.QuantityUsers);
            var tierDevice = FindTier(plan.PricingTiers, BillingUnit.Device, item.QuantityDevices);

            var unitPriceUsers   = tierUser?.PricePerUnit   ?? 0m;
            var unitPriceDevices = tierDevice?.PricePerUnit ?? 0m;
            var lineTotal        = unitPriceUsers * item.QuantityUsers + unitPriceDevices * item.QuantityDevices;

            lines.Add((item, plan, unitPriceUsers, unitPriceDevices, lineTotal));
        }

        // ── 2. Totaux ─────────────────────────────────────────────────────
        var subtotal  = lines.Sum(l => l.LineTotal);
        var taxAmount = subtotal * TaxRate;
        var total     = subtotal + taxAmount;

        // ── 3. Adresse ────────────────────────────────────────────────────
        var address = new Address
        {
            UserId       = userId,
            FirstName    = dto.Address.FirstName,
            LastName     = dto.Address.LastName,
            AddressLine1 = dto.Address.Line1,
            PostalCode   = dto.Address.PostalCode,
            City         = dto.Address.City,
            Country      = dto.Address.Country,
        };

        // ── 4. Commande + articles ────────────────────────────────────────
        var order = new Order
        {
            UserId                = userId,
            Status                = OrderStatus.Paid,
            TotalAmount           = total,
            StripePaymentIntentId = dto.StripePaymentIntentId,
        };

        foreach (var (item, plan, unitPriceUsers, unitPriceDevices, lineTotal) in lines)
        {
            var productName = plan.Product.Translations.FirstOrDefault(t => t.Locale == LocaleLang.Fr)?.Name
                           ?? plan.Product.Translations.FirstOrDefault()?.Name
                           ?? plan.Product.Slug;

            order.Items.Add(new OrderItem
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
        }

        // ── 5. Abonnements ────────────────────────────────────────────────
        var subscriptions = lines
            .Where(l => l.Plan.BillingPeriod != BillingPeriod.Lifetime)
            .Select(l => new Subscription
            {
                UserId               = userId,
                ProductId            = l.Plan.ProductId,
                PricingPlanId        = l.Plan.Id,
                StripeSubscriptionId = $"sub_mock_{Guid.NewGuid()}",
                Status               = SubscriptionStatus.Active,
                CurrentPeriodStart   = DateTime.UtcNow,
                CurrentPeriodEnd     = l.Plan.BillingPeriod == BillingPeriod.Yearly
                                       ? DateTime.UtcNow.AddYears(1)
                                       : DateTime.UtcNow.AddMonths(1),
                AutoRenew            = true,
            }).ToList();

        // ── 6. Persistance ────────────────────────────────────────────────
        await _orderRepository.SaveNewOrderAsync(address, order, subscriptions, userId);

        // ── 7. Réponse ────────────────────────────────────────────────────
        return new CreateOrderResultDto
        {
            Id                    = order.Id,
            Status                = order.Status.ToString().ToLowerInvariant(),
            Subtotal              = subtotal,
            TaxAmount             = taxAmount,
            Total                 = total,
            StripePaymentIntentId = dto.StripePaymentIntentId,
            CreatedAt             = order.CreatedAt,
            Items                 = lines.Zip(order.Items, (l, oi) => new CreateOrderResultItemDto
            {
                Id               = oi.Id,
                ProductName      = oi.ProductNameSnapshot,
                BillingPeriod    = l.Plan.BillingPeriod.ToString().ToLowerInvariant(),
                QuantityUsers    = oi.QuantityUsers,
                QuantityDevices  = oi.QuantityDevices,
                UnitPriceUsers   = l.UnitPriceUsers,
                UnitPriceDevices = l.UnitPriceDevices,
                LineTotal        = l.LineTotal,
            }).ToList(),
        };
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static PricingTier? FindTier(IEnumerable<PricingTier> tiers, BillingUnit unitType, int quantity)
    {
        if (quantity <= 0) return null;
        return tiers.FirstOrDefault(t =>
            t.unitType == unitType &&
            quantity >= t.minQuantity &&
            quantity <= t.maxQuantity);
    }
}