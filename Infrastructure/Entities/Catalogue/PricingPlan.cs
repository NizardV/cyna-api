namespace Infrastructure.Entities.Catalogue;

using OrdersAndSubscriptions;

public class PricingPlan
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public BillingPeriod BillingPeriod { get; set; }
    public decimal Price { get; set; }
    public int DiscountPercent { get; set; } = 0;
    public bool IsActive { get; set; } = true;

    public Product Product { get; set; } = null!;
    public ICollection<CartItem> CartItems { get; set; } = [];
    public ICollection<Subscription> Subscriptions { get; set; } = [];
    public ICollection<OrderItem> OrderItems { get; set; } = [];
}