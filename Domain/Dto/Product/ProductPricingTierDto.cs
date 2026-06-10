namespace Domain.Dto.Product;

/// <summary>
/// Représentation d'un palier de prix spécifique à la fiche produit.
/// </summary>
public class ProductPricingTierDto
{
    public string UnitType { get; set; } = string.Empty;
    public int MinQuantity { get; set; }
    public int MaxQuantity { get; set; }
    public decimal PricePerUnit { get; set; }
}