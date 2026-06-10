namespace Domain.Dto.Product;

/// <summary>
/// Représentation de la catégorie spécifique à la fiche produit (pour le fil d'ariane).
/// </summary>
public class ProductCategoryDto
{
    public int Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
}