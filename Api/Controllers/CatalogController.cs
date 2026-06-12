using Application.Interfaces;

using Domain.Dto.Catalog;

using Microsoft.AspNetCore.Mvc;

using NLog;

using ILogger = NLog.ILogger;

namespace Api.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class CatalogController : ControllerBase
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    private readonly ICatalogService _catalogService;

    public CatalogController(ICatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    /// <summary>
    /// Récupère les informations d'une catégorie et ses produits triés.
    /// </summary>
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

        // Log de la requête entrante
        _logger.Info(
            "GET /Catalog/category/{Slug} appelé — q={Q}, page={Page}",
            slug, q, page);

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