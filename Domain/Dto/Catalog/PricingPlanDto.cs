namespace Domain.Dto.Catalog;

/// <summary>
/// Représentation d'un plan tarifaire.
/// </summary>
public class PricingPlanDto
{
    /// <summary>Identifiant du plan.</summary>
    public int Id { get; set; }

    /// <summary>Nom du plan (ex. "Mensuel", "Annuel").</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Période de facturation (Monthly, Yearly, Lifetime).</summary>
    public string BillingPeriod { get; set; } = string.Empty;

    /// <summary>Pourcentage de réduction appliqué sur ce plan.</summary>
    public int DiscountPercent { get; set; }

    /// <summary>Paliers de prix associés à ce plan.</summary>
    public IEnumerable<PricingTierDto> PricingTiers { get; set; } = [];
}