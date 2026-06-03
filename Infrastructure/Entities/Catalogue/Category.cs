namespace Infrastructure.Entities;

public class Category
{
    public int Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; } = 0;

    public ICollection<CategoryTranslation> Translations { get; set; } = [];
    public ICollection<Product> Products { get; set; } = [];
}