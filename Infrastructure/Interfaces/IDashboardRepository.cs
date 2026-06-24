namespace Infrastructure.Interfaces;

using Domain.Dto.Dashboard;

/// <summary>
/// Interface du dépôt de statistiques du dashboard admin.
/// Effectue des requêtes d'agrégation directement via EF Core (count, sum, group by),
/// les dépôts existants (OrderRepository, etc.) n'étant pas conçus pour ce type de requête.
/// </summary>
public interface IDashboardRepository
{
    /// <summary>Calcule les statistiques de chiffre d'affaires (commandes payées) sur la période donnée.</summary>
    Task<RevenueStatsDto> GetRevenueStatsAsync(DateTime start, DateTime end, DateTime previousStart, DateTime previousEnd);

    /// <summary>Calcule les statistiques de commandes sur la période donnée.</summary>
    Task<OrderStatsDto> GetOrderStatsAsync(DateTime start, DateTime end);

    /// <summary>Calcule les statistiques d'utilisateurs sur la période donnée.</summary>
    Task<UserStatsDto> GetUserStatsAsync(DateTime start, DateTime end);

    /// <summary>Calcule les statistiques d'abonnements sur la période donnée.</summary>
    Task<SubscriptionStatsDto> GetSubscriptionStatsAsync(DateTime start, DateTime end);

    /// <summary>Récupère le top des produits par chiffre d'affaires ou nombre de commandes.</summary>
    Task<IEnumerable<TopProductDto>> GetTopProductsAsync(DateTime start, DateTime end, TopProductSortBy sortBy, int limit);
}