namespace Infrastructure.Entities.OrdersAndSubscriptions;

using Catalogue;

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int PricingPlanId { get; set; }
    public string ProductNameSnapshot { get; set; } = string.Empty;
    public string PlanNameSnapshot { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }

    public Order Order { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public PricingPlan PricingPlan { get; set; } = null!;
}