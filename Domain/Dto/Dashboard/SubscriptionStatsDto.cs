namespace Domain.Dto.Dashboard;

/// <summary>
/// Statistiques sur les abonnements pour le dashboard admin.
/// </summary>
/// <remarks>
/// NOTE TEMPORAIRE : voir <see cref="RevenueStatsDto"/> — les abonnements dépendent
/// eux aussi du module de paiement en cours de développement. Cette route accepte
/// <c>?mock=true</c> pour des données factices générées par Bogus en attendant.
/// </remarks>
public class SubscriptionStatsDto
{
    /// <summary>Nombre total d'abonnements créés sur la période sélectionnée.</summary>
    public int Total { get; set; }

    /// <summary>Nombre d'abonnements actuellement actifs (indépendant de la période).</summary>
    public int Active { get; set; }

    /// <summary>
    /// Répartition du nombre d'abonnements par statut.
    /// Clés en minuscules (ex. "active", "cancelled", "expired", "suspended", "pending").
    /// </summary>
    public IDictionary<string, int> ByStatus { get; set; } = new Dictionary<string, int>();

    /// <summary>Évolution du nombre d'abonnements créés, agrégée par mois.</summary>
    public IEnumerable<MonthlySubscriptionCountDto> ByMonth { get; set; } = [];
}

/// <summary>Point de données pour le graphique des abonnements créés par mois.</summary>
public class MonthlySubscriptionCountDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int Count { get; set; }
}