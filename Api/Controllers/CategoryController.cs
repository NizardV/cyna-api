using Microsoft.AspNetCore.Mvc;

using NLog;

namespace Api.Controllers;

using Application.Interfaces;

using Domain.Dto.Catalog;

using ILogger = NLog.ILogger;

/// <summary>
/// Contrôleur des catégories.
/// Expose la liste des catégories sur la route attendue par le front (/categories),
/// en complément de /recherche/categories.
/// </summary>
[ApiController]
[Route("categories")]
[Produces("application/json")]
public class CategoryController : ControllerBase
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    private readonly ICatalogService _catalogService;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="CategoryController"/>.
    /// </summary>
    /// <param name="catalogService">Le service catalogue.</param>
    public CategoryController(ICatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    // -------------------------------------------------------------------------
    // GET /categories
    // -------------------------------------------------------------------------

    /// <summary>
    /// Récupère toutes les catégories disponibles dans le catalogue.
    /// </summary>
    /// <param name="locale">Langue des traductions : fr | en (défaut : fr).</param>
    /// <returns>La liste des catégories triées par ordre d'affichage.</returns>
    /// <response code="200">Catégories récupérées avec succès.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategories([FromQuery] string locale = "fr")
    {
        _logger.Info("GET /categories — locale={Locale}", locale);

        var categories = await _catalogService.GetCategoriesAsync(locale);
        return Ok(categories);
    }
}
