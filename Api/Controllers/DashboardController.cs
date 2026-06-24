using Microsoft.AspNetCore.Mvc;

using NLog;

namespace Api.Controllers;

using Application.Interfaces;

using Domain.Dto.Dashboard;

using Infrastructure.Entities;

using ILogger = NLog.ILogger;

/// <summary>
/// Contrôleur d'administration du dashboard.
/// Expose les statistiques globales de la plateforme.
///
/// Toutes les routes supportent le paramètre <c>?mock=true</c>.
///
/// Lorsque ce paramètre est activé, des données fictives cohérentes sont
/// générées via Bogus afin de permettre le développement du front-end,
/// les tests fonctionnels et les démonstrations.
///
/// Ce mode existe temporairement car l'intégration des paiements et la
/// collecte des données métier sont développées en parallèle et les
/// données réelles ne sont pas encore disponibles dans tous les
/// environnements.
/// </summary>
[ApiController]
[Route("dashboard")]
[Produces("application/json")]
// [Authorize(Roles = "Admin,SuperAdmin")]
public class DashboardController : ControllerBase
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    private readonly IDashboardService _service;

    public DashboardController(IDashboardService service)
    {
        _service = service;
    }

    // -------------------------------------------------------------------------
    // GET /dashboard/ca
    // -------------------------------------------------------------------------

    /// <summary>
    /// Récupère les statistiques de chiffre d'affaires.
    /// </summary>
    /// <param name="filter">
    /// Filtres de période (période prédéfinie ou plage de dates personnalisée).
    /// </param>
    /// <param name="mock">
    /// Génère des données fictives cohérentes via Bogus.
    /// </param>
    /// <response code="200">Statistiques récupérées avec succès.</response>
    [HttpGet("ca")]
    [ProducesResponseType(typeof(RevenueStatsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRevenue(
        [FromQuery] DashboardFilterDto filter,
        [FromQuery] bool mock = false)
    {
        _logger.Info(
            "GET /dashboard/ca — mock={Mock}",
            mock);

        var result = await _service.GetRevenueStatsAsync(filter, mock);

        return Ok(result);
    }

    // -------------------------------------------------------------------------
    // GET /dashboard/orders
    // -------------------------------------------------------------------------

    /// <summary>
    /// Récupère les statistiques des commandes.
    /// </summary>
    /// <param name="filter">
    /// Filtres de période (période prédéfinie ou plage de dates personnalisée).
    /// </param>
    /// <param name="mock">
    /// Génère des données fictives cohérentes via Bogus.
    /// </param>
    /// <response code="200">Statistiques récupérées avec succès.</response>
    [HttpGet("orders")]
    [ProducesResponseType(typeof(OrderStatsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrders(
        [FromQuery] DashboardFilterDto filter,
        [FromQuery] bool mock = false)
    {
        _logger.Info(
            "GET /dashboard/orders — mock={Mock}",
            mock);

        var result = await _service.GetOrderStatsAsync(filter, mock);

        return Ok(result);
    }

    // -------------------------------------------------------------------------
    // GET /dashboard/users
    // -------------------------------------------------------------------------

    /// <summary>
    /// Récupère les statistiques des utilisateurs.
    /// </summary>
    /// <param name="filter">
    /// Filtres de période (période prédéfinie ou plage de dates personnalisée).
    /// </param>
    /// <param name="mock">
    /// Génère des données fictives cohérentes via Bogus.
    /// </param>
    /// <response code="200">Statistiques récupérées avec succès.</response>
    [HttpGet("users")]
    [ProducesResponseType(typeof(UserStatsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsers(
        [FromQuery] DashboardFilterDto filter,
        [FromQuery] bool mock = false)
    {
        _logger.Info(
            "GET /dashboard/users — mock={Mock}",
            mock);

        var result = await _service.GetUserStatsAsync(filter, mock);

        return Ok(result);
    }

    // -------------------------------------------------------------------------
    // GET /dashboard/subscriptions
    // -------------------------------------------------------------------------

    /// <summary>
    /// Récupère les statistiques des abonnements.
    /// </summary>
    /// <param name="filter">
    /// Filtres de période (période prédéfinie ou plage de dates personnalisée).
    /// </param>
    /// <param name="mock">
    /// Génère des données fictives cohérentes via Bogus.
    /// </param>
    /// <response code="200">Statistiques récupérées avec succès.</response>
    [HttpGet("subscriptions")]
    [ProducesResponseType(typeof(SubscriptionStatsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSubscriptions(
        [FromQuery] DashboardFilterDto filter,
        [FromQuery] bool mock = false)
    {
        _logger.Info(
            "GET /dashboard/subscriptions — mock={Mock}",
            mock);

        var result = await _service.GetSubscriptionStatsAsync(filter, mock);

        return Ok(result);
    }

    // -------------------------------------------------------------------------
    // GET /dashboard/products/top
    // -------------------------------------------------------------------------

    /// <summary>
    /// Récupère les produits les plus performants.
    /// </summary>
    /// <param name="filter">
    /// Filtres de période (période prédéfinie ou plage de dates personnalisée).
    /// </param>
    /// <param name="sortBy">
    /// Critère de tri :
    /// Revenue (défaut) ou Orders.
    /// </param>
    /// <param name="limit">
    /// Nombre maximum d'éléments retournés.
    /// </param>
    /// <param name="mock">
    /// Génère des données fictives cohérentes via Bogus.
    /// </param>
    /// <response code="200">Produits récupérés avec succès.</response>
    /// <response code="400">Paramètres invalides.</response>
    [HttpGet("products/top")]
    [ProducesResponseType(typeof(IEnumerable<TopProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetTopProducts(
        [FromQuery] DashboardFilterDto filter,
        [FromQuery] TopProductSortBy sortBy = TopProductSortBy.Revenue,
        [FromQuery] int limit = 10,
        [FromQuery] bool mock = false)
    {
        if (limit < 1)
            return BadRequest(new
            {
                error = "La limite doit être supérieure à 0."
            });

        _logger.Info(
            "GET /dashboard/products/top — sortBy={SortBy}, limit={Limit}, mock={Mock}",
            sortBy,
            limit,
            mock);

        var result = await _service.GetTopProductsAsync(
            filter,
            sortBy,
            limit,
            mock);

        return Ok(result);
    }
}