namespace Domain.Dto.Dashboard;

/// <summary>
/// Statistiques sur les commandes pour le dashboard admin.
/// </summary>
/// <remarks>
/// NOTE TEMPORAIRE : voir <see cref="RevenueStatsDto"/> — le module de paiement étant
/// développé en parallèle, cette route accepte <c>?mock=true</c> pour des données factices
/// générées par Bogus tant que les commandes réelles ne sont pas fiables en base.
/// </remarks>
public class OrderStatsDto
{
    /// <summary>Nombre total de commandes sur la période sélectionnée.</summary>
    public int Total { get; set; }

    /// <summary>
    /// Répartition du nombre de commandes par statut.
    /// Clés en minuscules pour correspondre directement aux valeurs attendues côté front
    /// (ex. "pending", "paid", "failed", "refunded", "cancelled").
    /// </summary>
    public IDictionary<string, int> ByStatus { get; set; } = new Dictionary<string, int>();

    /// <summary>Évolution du nombre de commandes, agrégée par mois, pour affichage en graphique.</summary>
    public IEnumerable<MonthlyOrderCountDto> ByMonth { get; set; } = [];
}

/// <summary>Point de données pour le graphique du nombre de commandes par mois.</summary>
public class MonthlyOrderCountDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int Count { get; set; }
}