namespace Infrastructure.Entities.Catalogue;

using System.ComponentModel.DataAnnotations;

public class PricingTier
{
    public int Id { get; set; }
    public int PricingPlanId { get; set; }

    [Required]
    public BillingUnit unitType { get; set; }

    [Required]
    public int minQuantity { get; set; }

    [Required]
    public int maxQuantity { get; set; }

    [Required]
    [DataType(DataType.Currency)]
    public decimal PricePerUnit { get; set; }

    public PricingPlan PricingPlan { get; set; } = null!;
}