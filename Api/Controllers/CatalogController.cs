using Application.Interfaces;

using Domain.Dto.Catalog;

using Microsoft.AspNetCore.Mvc;

using NLog;

using ILogger = NLog.ILogger;

namespace Api.Controllers;

/// <summary>
/// Contrôleur du catalogue produits par catégorie.
/// Expose la navigation par catégorie avec filtres, pagination et tri automatique (Catalog Priority).
/// </summary>
[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class CatalogController : ControllerBase
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    private readonly ICatalogService _catalogService;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="CatalogController"/>.
    /// </summary>
    /// <param name="catalogService">Le service catalogue.</param>
    public CatalogController(ICatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    /// <summary>
    /// Récupère les informations d'une catégorie et ses produits triés selon l'algorithme Catalog Priority.
    /// </summary>
    /// <param name="slug">Le slug unique de la catégorie.</param>
    /// <param name="q">Recherche textuelle dans le nom et la description des produits (optionnel).</param>
    /// <param name="maxPrice">Prix unitaire mensuel maximum en euros (optionnel).</param>
    /// <param name="available">Si true, retourne uniquement les produits disponibles (défaut : false).</param>
    /// <param name="page">Numéro de page, base 1 (défaut : 1).</param>
    /// <param name="pageSize">Nombre de produits par page, max 100 (défaut : 9).</param>
    /// <param name="locale">Langue des traductions : fr | en (défaut : fr).</param>
    /// <returns>La bannière de la catégorie et la page de produits avec métadonnées de pagination.</returns>
    /// <response code="200">Catalogue retourné avec succès.</response>
    /// <response code="400">Paramètres de pagination invalides.</response>
    /// <response code="404">Catégorie introuvable pour le slug donné.</response>
    [HttpGet("category/{slug}")]
    [ProducesResponseType(typeof(CategoryCatalogPageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCategoryCatalog(
        string slug,
        [FromQuery] string? q = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] bool available = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 9,
        [FromQuery] string locale = "fr")
    {
        if (page < 1)
        {
            _logger.Warn("Requête bloquée : numéro de page invalide ({Page})", page);
            return BadRequest(new { error = "Le numéro de page doit être supérieur ou égal à 1." });
        }

        if (pageSize < 1 || pageSize > 100)
        {
            _logger.Warn("Requête bloquée : taille de page invalide ({PageSize})", pageSize);
            return BadRequest(new { error = "La taille de page doit être comprise entre 1 et 100." });
        }

        _logger.Info("GET /Catalog/category/{Slug} — q={Q}, page={Page}", slug, q, page);

        try
        {
            var result = await _catalogService.GetCategoryCatalogAsync(
                slug, q, maxPrice, available, page, pageSize, locale);

            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}