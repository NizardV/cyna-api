namespace Domain.Dto.Catalog;

/// <summary>
/// Représentation d'un palier de prix au sein d'un plan tarifaire.
/// </summary>
public class PricingTierDto
{
    /// <summary>Type d'unité facturée (User, Device).</summary>
    public string UnitType { get; set; } = string.Empty;

    /// <summary>Quantité minimale pour ce palier.</summary>
    public int MinQuantity { get; set; }

    /// <summary>Quantité maximale pour ce palier.</summary>
    public int MaxQuantity { get; set; }

    /// <summary>Prix unitaire pour ce palier.</summary>
    public decimal PricePerUnit { get; set; }
}