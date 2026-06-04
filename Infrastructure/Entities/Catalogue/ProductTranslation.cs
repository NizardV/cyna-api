namespace Infrastructure.Entities.Catalogue;

public class ProductTranslation
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public LocaleLang Locale { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public Product Product { get; set; } = null!;
}