namespace Infrastructure.Entities.OrdersAndSubscriptions;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Catalogue;

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int PricingPlanId { get; set; }

    [Required, MaxLength(200)]
    public string ProductNameSnapshot { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string PlanNameSnapshot { get; set; } = string.Empty;

    public int QuantityUsers { get; set; }
    public int QuantityDevices { get; set; }

    public Order Order { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public PricingPlan PricingPlan { get; set; } = null!;
}