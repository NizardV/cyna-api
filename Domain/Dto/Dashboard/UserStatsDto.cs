namespace Domain.Dto.Dashboard;

/// <summary>
/// Statistiques sur les utilisateurs pour le dashboard admin.
/// </summary>
public class UserStatsDto
{
    /// <summary>Nombre total d'utilisateurs (tous rôles confondus).</summary>
    public int Total { get; set; }

    /// <summary>Nombre de nouveaux utilisateurs inscrits sur la période sélectionnée.</summary>
    public int NewInPeriod { get; set; }

    /// <summary>Nombre d'utilisateurs avec l'e-mail vérifié.</summary>
    public int VerifiedEmail { get; set; }

    /// <summary>Évolution du nombre de nouvelles inscriptions, agrégée par mois.</summary>
    public IEnumerable<MonthlyUserCountDto> ByMonth { get; set; } = [];
}

/// <summary>Point de données pour le graphique des nouvelles inscriptions par mois.</summary>
public class MonthlyUserCountDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int Count { get; set; }
}