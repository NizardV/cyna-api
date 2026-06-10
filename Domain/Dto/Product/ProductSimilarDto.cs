namespace Domain.Dto.Product;

/// <summary>
/// DTO représentant une carte allégée pour les produits similaires en bas de page.
/// </summary>
public class ProductSimilarDto
{
    public int Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public decimal? Price { get; set; }
}