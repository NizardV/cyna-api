namespace Domain.Dto.Dashboard;

/// <summary>
/// Statistiques de chiffre d'affaires pour le dashboard admin.
/// Calculées à partir des commandes au statut "Paid" uniquement.
/// </summary>
/// <remarks>
/// NOTE TEMPORAIRE : le module de paiement (Stripe) est en cours d'implémentation
/// en parallèle par un autre développeur. En attendant des données de paiement réelles
/// et fiables en base, cette route accepte <c>?mock=true</c> pour retourner des données
/// générées par Bogus, afin que le front (dashboard admin) puisse être développé et
/// démontré sans dépendre de l'avancement du module paiement. Une fois le module de
/// paiement stabilisé, retirer le paramètre mock ou le laisser en fallback de dev.
/// </remarks>
public class RevenueStatsDto
{
    /// <summary>Chiffre d'affaires total sur la période sélectionnée.</summary>
    public decimal Total { get; set; }

    /// <summary>Chiffre d'affaires de la période courante (ex. mois en cours selon le filtre).</summary>
    public decimal CurrentPeriod { get; set; }

    /// <summary>Chiffre d'affaires de la période précédente équivalente (pour comparaison).</summary>
    public decimal PreviousPeriod { get; set; }

    /// <summary>Évolution en pourcentage entre la période précédente et la période courante.</summary>
    public decimal GrowthPercent { get; set; }

    /// <summary>Historique du chiffre d'affaires, agrégé par mois, pour affichage en graphique.</summary>
    public IEnumerable<MonthlyRevenueDto> ByMonth { get; set; } = [];
}

/// <summary>Point de données pour le graphique de chiffre d'affaires mensuel.</summary>
public class MonthlyRevenueDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal Revenue { get; set; }
}