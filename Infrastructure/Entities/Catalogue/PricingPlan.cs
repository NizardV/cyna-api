namespace Infrastructure.Entities.Catalogue;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OrdersAndSubscriptions;

public class PricingPlan
{
    public int Id { get; set; }
    public int ProductId { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public BillingPeriod BillingPeriod { get; set; }
    public int DiscountPercent { get; set; } = 0;

    public Product Product { get; set; } = null!;
    public ICollection<PricingTier> PricingTiers { get; set; } = [];
    public ICollection<CartItem> CartItems { get; set; } = [];
    public ICollection<Subscription> Subscriptions { get; set; } = [];
    public ICollection<OrderItem> OrderItems { get; set; } = [];
}