namespace Domain.Entities.Catalogue;

using System.ComponentModel.DataAnnotations;

using OrdersAndSubscriptions;

using Tools;

public class PricingPlan
{
    public int Id { get; set; }
    public int ProductId { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public BillingPeriod BillingPeriod { get; set; }
    public int DiscountPercent { get; set; } = 0;

    /// <summary>Quantité maximale d'utilisateurs autorisée au checkout pour ce plan.</summary>
    public int MaxUsersCheckout { get; set; } = 999;

    /// <summary>Quantité maximale d'appareils autorisée au checkout pour ce plan.</summary>
    public int MaxDevicesCheckout { get; set; } = 999;

    public Product Product { get; set; } = null!;
    public ICollection<PricingTier> PricingTiers { get; set; } = [];
    public ICollection<CartItem> CartItems { get; set; } = [];
    public ICollection<Subscription> Subscriptions { get; set; } = [];
    public ICollection<OrderItem> OrderItems { get; set; } = [];
}