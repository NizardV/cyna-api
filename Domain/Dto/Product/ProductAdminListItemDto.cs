namespace Domain.Dto.Product;

/// <summary>
/// Ligne de la liste des produits du back-office.
/// </summary>
public class ProductAdminListItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsFeatured { get; set; }
    public int? DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
}
