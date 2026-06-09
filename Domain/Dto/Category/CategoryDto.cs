namespace Domain.Dto.Category;

/// <summary>
/// Catégorie enrichie pour l'interface.
/// Inclut le nombre de produits associés.
/// </summary>
public class CategoryDto
{
    public int Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public int ProductCount { get; set; }
}