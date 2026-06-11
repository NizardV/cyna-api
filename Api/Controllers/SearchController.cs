using Microsoft.AspNetCore.Mvc;
using NLog;

namespace Api.Controllers;

using Application.Interfaces;

using Domain.Dto.Catalog;

using ILogger = NLog.ILogger;

/// <summary>
/// Contrôleur de recherche et de catalogue produits.
/// Expose les routes publiques de recherche, filtrage et récupération des catégories.
/// </summary>
[ApiController]
[Route("Search")]
[Produces("application/json")]
public class SearchController : ControllerBase
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    private readonly ICatalogService _catalogService;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="SearchController"/>.
    /// </summary>
    /// <param name="catalogService">Le service catalogue.</param>
    public SearchController(ICatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    // -------------------------------------------------------------------------
    // GET /Search
    // -------------------------------------------------------------------------

    /// <summary>
    /// Recherche les produits du catalogue avec filtres, tri et pagination.
    /// </summary>
    /// <param name="q">Recherche textuelle sur le nom et la description (optionnel).</param>
    /// <param name="categoryIds">Identifiants de catégories séparés par des virgules (optionnel).</param>
    /// <param name="maxPrice">Prix maximum mensuel en euros (optionnel).</param>
    /// <param name="available">Si true, retourne uniquement les produits disponibles (défaut : false).</param>
    /// <param name="sortBy">Critère de tri : relevance | price_asc | price_desc | name (défaut : relevance).</param>
    /// <param name="page">Numéro de page, base 1 (défaut : 1).</param>
    /// <param name="pageSize">Nombre d'éléments par page (défaut : 9).</param>
    /// <param name="locale">Langue des traductions : fr | en (défaut : fr).</param>
    /// <returns>La page de résultats avec métadonnées de pagination.</returns>
    /// <response code="200">Résultats retournés avec succès.</response>
    /// <response code="400">Paramètres de requête invalides.</response>
    [HttpGet]
    [ProducesResponseType(typeof(CatalogPageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetCatalog(
        [FromQuery] string? q           = null,
        [FromQuery] string? categoryIds = null,
        [FromQuery] decimal? maxPrice   = null,
        [FromQuery] bool available      = false,
        [FromQuery] string sortBy       = "relevance",
        [FromQuery] int page            = 1,
        [FromQuery] int pageSize        = 9,
        [FromQuery] string locale       = "fr")
    {
        if (page < 1)
        {
            return BadRequest(new { error = "Le numéro de page doit être supérieur ou égal à 1." });
        }

        if (pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new { error = "La taille de page doit être comprise entre 1 et 100." });
        }

        _logger.Info(
            "GET /recherche/catalog — q={Q}, page={Page}, pageSize={PageSize}, sortBy={SortBy}",
            q, page, pageSize, sortBy);

        var result = await _catalogService.GetProductsAsync(
            q, categoryIds, maxPrice, available, sortBy, page, pageSize, locale);

        return Ok(result);
    }
}