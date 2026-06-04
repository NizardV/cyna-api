namespace Infrastructure.Entities;

using Catalogue;

public class CartItem
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ProductId { get; set; }
    public int PricingPlanId { get; set; }

    public int quantityUsers { get; set; }
    public int quantityDevices { get; set; }

    public User User { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public PricingPlan PricingPlan { get; set; } = null!;
}