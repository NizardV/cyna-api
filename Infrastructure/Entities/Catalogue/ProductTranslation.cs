namespace Infrastructure.Entities.Catalogue;

using System.ComponentModel.DataAnnotations;

public class ProductTranslation
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public LocaleLang Locale { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    public Product Product { get; set; } = null!;
}