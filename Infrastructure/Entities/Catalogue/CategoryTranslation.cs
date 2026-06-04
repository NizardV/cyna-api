namespace Infrastructure.Entities.Catalogue;

using System.ComponentModel.DataAnnotations;

public class CategoryTranslation
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public LocaleLang Locale { get; set; }

    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Category Category { get; set; } = null!;
}