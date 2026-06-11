namespace Domain.Dto.Category;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Traduction d'une catégorie pour une locale donnée.
/// </summary>
public class CategoryTranslationDto
{
    /// <summary>Locale : "fr" ou "en".</summary>
    [Required]
    [RegularExpression("^(fr|en)$", ErrorMessage = "La locale doit être 'fr' ou 'en'.")]
    public string Locale { get; set; } = "fr";

    /// <summary>Nom traduit de la catégorie.</summary>
    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Description traduite (optionnelle).</summary>
    public string? Description { get; set; }
}