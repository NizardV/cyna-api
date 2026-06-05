namespace Domain.Entities.Catalogue;

using System.ComponentModel.DataAnnotations;

public class Category
{
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string Slug { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; } = 0;

    public ICollection<CategoryTranslation> Translations { get; set; } = [];
    public ICollection<Product> Products { get; set; } = [];
}