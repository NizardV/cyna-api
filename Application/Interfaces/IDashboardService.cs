namespace Application.Interfaces;

using Domain.Dto.Dashboard;
using Infrastructure.Entities;

/// <summary>
/// Interface du service de statistiques du dashboard.
/// </summary>
public interface IDashboardService
{
    /// <summary>
    /// Récupère les statistiques de chiffre d'affaires.
    /// </summary>
    Task<RevenueStatsDto> GetRevenueStatsAsync(
        DashboardFilterDto filter,
        bool mock);

    /// <summary>
    /// Récupère les statistiques des commandes.
    /// </summary>
    Task<OrderStatsDto> GetOrderStatsAsync(
        DashboardFilterDto filter,
        bool mock);

    /// <summary>
    /// Récupère les statistiques des utilisateurs.
    /// </summary>
    Task<UserStatsDto> GetUserStatsAsync(
        DashboardFilterDto filter,
        bool mock);

    /// <summary>
    /// Récupère les statistiques des abonnements.
    /// </summary>
    Task<SubscriptionStatsDto> GetSubscriptionStatsAsync(
        DashboardFilterDto filter,
        bool mock);

    /// <summary>
    /// Récupère les produits les plus performants.
    /// </summary>
    Task<IEnumerable<TopProductDto>> GetTopProductsAsync(
        DashboardFilterDto filter,
        TopProductSortBy sortBy,
        int limit,
        bool mock);
}