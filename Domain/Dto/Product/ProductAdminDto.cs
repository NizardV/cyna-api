namespace Domain.Dto.Product;

/// <summary>
/// DTO complet d'un produit pour le back-office : les deux locales et tous les champs éditables.
/// </summary>
public class ProductAdminDto
{
    public int Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string NameFr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string DescriptionFr { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsFeatured { get; set; }
    public int? DisplayOrder { get; set; }
    public IEnumerable<string> TechnicalSpecs { get; set; } = [];
    public IEnumerable<ProductAdminPricingPlanDto> PricingPlans { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Plan tarifaire au format attendu par le formulaire admin (période en minuscules).
/// </summary>
public class ProductAdminPricingPlanDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string BillingPeriod { get; set; } = string.Empty;
    public int DiscountPercent { get; set; }
    public int MaxUsersCheckout { get; set; }
    public int MaxDevicesCheckout { get; set; }
    public IEnumerable<ProductAdminPricingTierDto> PricingTiers { get; set; } = [];
}

/// <summary>
/// Palier de prix au format attendu par le formulaire admin (unité en minuscules).
/// </summary>
public class ProductAdminPricingTierDto
{
    public string UnitType { get; set; } = string.Empty;
    public int MinQty { get; set; }
    public int MaxQty { get; set; }
    public decimal UnitPrice { get; set; }
}
