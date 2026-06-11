namespace Domain.Dto.Product;

/// <summary>
/// Représentation d'un palier de prix spécifique à la fiche produit.
/// </summary>
public class ProductPricingTierDto
{
    public string UnitType { get; set; } = string.Empty;
    public int MinQty { get; set; }
    public int MaxQty { get; set; }
    public decimal UnitPrice { get; set; }
}