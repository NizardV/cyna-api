namespace Domain.Dto.User;

/// <summary>
/// Résumé d'un abonnement actif de l'utilisateur.
/// </summary>
public class SubscriptionDto
{
    /// <summary>Identifiant de l'abonnement.</summary>
    public int Id { get; set; }

    /// <summary>Statut de l'abonnement (Active, Cancelled, Expired, …).</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Nom traduit du produit souscrit.</summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>Nom du plan tarifaire souscrit.</summary>
    public string PlanName { get; set; } = string.Empty;

    /// <summary>Début de la période de facturation courante.</summary>
    public DateTime CurrentPeriodStart { get; set; }

    /// <summary>Fin de la période de facturation courante.</summary>
    public DateTime CurrentPeriodEnd { get; set; }

    /// <summary>Indique si le renouvellement automatique est activé.</summary>
    public bool AutoRenew { get; set; }
}