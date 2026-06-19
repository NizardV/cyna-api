namespace Domain.Entities.Catalogue;

using System.ComponentModel.DataAnnotations;

public class ProductImage
{
    public int Id { get; set; }
    public int ProductId { get; set; }

    [Required, MaxLength(255)]
    public string ImageUrl { get; set; } = string.Empty;

    public int DisplayOrder { get; set; } = 0;

    public Product Product { get; set; } = null!;
}