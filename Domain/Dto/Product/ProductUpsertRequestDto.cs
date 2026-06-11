namespace Domain.Dto.Product;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Corps de requête pour la création ou la mise à jour complète d'un produit (back-office).
/// Le slug est généré côté serveur à partir du nom français.
/// </summary>
public class ProductUpsertRequestDto
{
    [Required, MaxLength(200)]
    public string NameFr { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? NameEn { get; set; }

    public string? DescriptionFr { get; set; }

    public string? DescriptionEn { get; set; }

    /// <summary>Statut produit : Available | Unavailable | OutOfStock | Preview (insensible à la casse).</summary>
    [Required]
    public string Status { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }

    [MaxLength(255)]
    public string? ImageUrl { get; set; }

    public bool IsFeatured { get; set; }

    /// <summary>Ordre d'affichage du produit mis en avant (ignoré si IsFeatured est false).</summary>
    [Range(1, int.MaxValue)]
    public int? DisplayOrder { get; set; }

    /// <summary>Spécifications techniques (une chaîne par ligne, stockées en JSON).</summary>
    public List<string> TechnicalSpecs { get; set; } = [];

    public List<ProductPricingPlanInputDto> PricingPlans { get; set; } = [];
}

/// <summary>
/// Plan tarifaire tel que soumis par le formulaire admin.
/// </summary>
public class ProductPricingPlanInputDto
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Période de facturation : monthly | yearly | lifetime (insensible à la casse).</summary>
    [Required]
    public string BillingPeriod { get; set; } = string.Empty;

    [Range(0, 100)]
    public int DiscountPercent { get; set; }

    [Range(1, int.MaxValue)]
    public int MaxUsersCheckout { get; set; } = 999;

    [Range(1, int.MaxValue)]
    public int MaxDevicesCheckout { get; set; } = 999;

    public List<ProductPricingTierInputDto> PricingTiers { get; set; } = [];
}

/// <summary>
/// Palier de prix tel que soumis par le formulaire admin.
/// </summary>
public class ProductPricingTierInputDto
{
    /// <summary>Type d'unité : user | device (insensible à la casse).</summary>
    [Required]
    public string UnitType { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int MinQty { get; set; }

    [Range(1, int.MaxValue)]
    public int MaxQty { get; set; }

    [Range(0, double.MaxValue)]
    public decimal UnitPrice { get; set; }
}
