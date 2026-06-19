namespace Domain.Dto.Catalog;

/// <summary>
/// Représentation d'un produit dans le catalogue.
/// </summary>
public class ProductDto
{
    /// <summary>Identifiant du produit.</summary>
    public int Id { get; set; }

    /// <summary>Nom traduit du produit.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Description traduite du produit.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Statut du produit (Available, Unavailable, …).</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>URL de la première image du produit.</summary>
    public string? ImageUrl { get; set; }

    /// <summary>Plans tarifaires disponibles pour ce produit.</summary>
    public decimal Price { get; set; }
}