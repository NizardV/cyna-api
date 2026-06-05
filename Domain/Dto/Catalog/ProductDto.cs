namespace Domain.Dto.Catalog;

/// <summary>
/// Représentation d'un produit dans le catalogue.
/// </summary>
public class ProductDto
{
    /// <summary>Identifiant du produit.</summary>
    public int Id { get; set; }

    /// <summary>Slug unique du produit.</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>Nom traduit du produit.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Description traduite du produit.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Statut du produit (Available, Unavailable, …).</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Indique si le produit est mis en avant.</summary>
    public bool IsFeatured { get; set; }

    /// <summary>Identifiant de la catégorie parente.</summary>
    public int CategoryId { get; set; }

    /// <summary>Nom traduit de la catégorie parente.</summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>URL de la première image du produit.</summary>
    public string? ImageUrl { get; set; }

    /// <summary>Plans tarifaires disponibles pour ce produit.</summary>
    public IEnumerable<PricingPlanDto> PricingPlans { get; set; } = [];
}