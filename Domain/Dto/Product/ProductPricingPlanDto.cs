namespace Domain.Dto.Product;

/// <summary>
/// Représentation d'un plan tarifaire spécifique à la fiche produit.
/// </summary>
public class ProductPricingPlanDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string BillingPeriod { get; set; } = string.Empty;
    public int DiscountPercent { get; set; }
    public IEnumerable<ProductPricingTierDto> PricingTiers { get; set; } = [];
}

