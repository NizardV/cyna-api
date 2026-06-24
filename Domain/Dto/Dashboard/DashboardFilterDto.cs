namespace Domain.Dto.Dashboard;

/// <summary>
/// Période prédéfinie pour le filtrage des statistiques du dashboard.
/// </summary>
public enum DashboardPeriod
{
    Week,
    Month,
    Year,
    All,
}

/// <summary>
/// Paramètres de filtrage temporel communs aux routes du dashboard.
/// Si <see cref="From"/> et <see cref="To"/> sont fournis, ils priment sur <see cref="Period"/>.
/// </summary>
public class DashboardFilterDto
{
    /// <summary>Période prédéfinie (week | month | year | all). Défaut : month.</summary>
    public DashboardPeriod Period { get; set; } = DashboardPeriod.Month;

    /// <summary>Date de début (incluse), prioritaire sur <see cref="Period"/> si fournie avec <see cref="To"/>.</summary>
    public DateTime? From { get; set; }

    /// <summary>Date de fin (incluse), prioritaire sur <see cref="Period"/> si fournie avec <see cref="From"/>.</summary>
    public DateTime? To { get; set; }

    /// <summary>
    /// Résout les bornes effectives [Start, End) à utiliser dans les requêtes,
    /// en combinant From/To (s'ils sont tous les deux définis) ou la période prédéfinie.
    /// </summary>
    public (DateTime Start, DateTime End) Resolve()
    {
        var now = DateTime.UtcNow;

        if (From.HasValue && To.HasValue)
        {
            // Bound inclusive on To by pushing to the end of that day.
            return (From.Value.Date, To.Value.Date.AddDays(1));
        }

        return Period switch
        {
            DashboardPeriod.Week  => (now.AddDays(-7), now),
            DashboardPeriod.Month => (now.AddMonths(-1), now),
            DashboardPeriod.Year  => (now.AddYears(-1), now),
            DashboardPeriod.All   => (DateTime.MinValue, now),
            _                     => (now.AddMonths(-1), now),
        };
    }
}