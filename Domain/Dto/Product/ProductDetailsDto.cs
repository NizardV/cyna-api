namespace Domain.Dto.Product;

/// <summary>
/// DTO central représentant les détails complets et isolés d'un produit.
/// </summary>
public class ProductDetailsDto
{
    public int Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? TechnicalSpecs { get; set; }
    public string Status { get; set; } = string.Empty;

    public ProductCategoryDto Category { get; set; } = null!;
    public IEnumerable<string> Images { get; set; } = [];
    public IEnumerable<ProductPricingPlanDto> PricingPlans { get; set; } = [];
}