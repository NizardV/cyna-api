namespace Infrastructure.Entities.Catalogue;

public class CategoryTranslation
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public LocaleLang Locale { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public Category Category { get; set; } = null!;
}