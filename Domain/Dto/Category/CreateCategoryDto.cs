namespace Domain.Dto.Category;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Corps de requête pour la création d'une catégorie.
/// </summary>
public class CreateCategoryDto
{
    /// <summary>Slug unique (généré depuis le nom si absent).</summary>
    public string? Slug { get; set; }

    /// <summary>URL de l'image de la catégorie.</summary>
    public string? ImageUrl { get; set; }

    /// <summary>Ordre d'affichage dans le catalogue.</summary>
    public int DisplayOrder { get; set; } = 0;

    /// <summary>Traductions de la catégorie (au moins une requise).</summary>
    [Required, MinLength(1)]
    public IEnumerable<CategoryTranslationDto> Translations { get; set; } = [];
}
