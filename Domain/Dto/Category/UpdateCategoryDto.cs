namespace Domain.Dto.Category;

/// <summary>
/// Corps de requête pour la mise à jour d'une catégorie.
/// Tous les champs sont optionnels — seuls ceux fournis sont mis à jour.
/// </summary>
public class UpdateCategoryDto
{
    public string? Slug { get; set; }
    public string? ImageUrl { get; set; }
    public int? DisplayOrder { get; set; }
    public IEnumerable<CategoryTranslationDto>? Translations { get; set; }
}
