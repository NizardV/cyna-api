namespace Domain.Dto.Catalog;

/// <summary>
/// Résultat paginé d'une catégorie avec les informations de son en-tête.
/// </summary>
public class CategoryCatalogPageDto : CatalogPageDto
{
    public string CategoryName { get; set; } = string.Empty;
    public string? CategoryDescription { get; set; }
    public string? CategoryImageUrl { get; set; }
}