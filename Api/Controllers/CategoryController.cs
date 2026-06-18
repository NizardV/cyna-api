using Microsoft.AspNetCore.Mvc;
using NLog;

namespace Api.Controllers;

using Application.Interfaces;

using Domain.Dto.Category;

using Microsoft.AspNetCore.Authorization;

using Tools;

using ILogger = NLog.ILogger;

/// <summary>
/// Contrôleur d'administration des catégories.
/// Expose les routes CRUD protégées (Admin uniquement).
/// </summary>
[ApiController]
[Route("categories")]
[Produces("application/json")]
[Authorize(Roles = "Admin")]
public class CategoryController : ControllerBase
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    private readonly ICategoryService _service;

    public CategoryController(ICategoryService service)
    {
        _service = service;
    }

    /// <summary>
    /// Validates every <see cref="CategoryTranslationDto"/> inside a payload.
    /// Returns an error message on the first violation, or <c>null</c> if valid.
    /// </summary>
    private static string? ValidateTranslationLocales(IEnumerable<CategoryTranslationDto>? translations)
    {
        if (translations is null) return null;

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var t in translations)
        {
            if (i18n.ParseLocale(t.Locale) is null)
                return $"Locale inconnue « {t.Locale} ». Valeurs acceptées : fr, en.";

            if (!seen.Add(t.Locale.ToLower()))
                return $"La locale « {t.Locale} » est présente plusieurs fois.";
        }
        return null;
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
    /// <response code="400">Locale inconnue.</response>
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetCategories([FromQuery] string locale = "fr")
    {
        if (i18n.ParseLocale(locale) is not { } parsedLocale)
            return BadRequest(new { error = $"Locale inconnue « {locale} ». Valeurs acceptées : fr, en." });

        _logger.Info("GET /categories — locale={Locale}", locale);

        var categories = await _service.GetCategoriesAsync(parsedLocale);
        return Ok(categories);
    }

    // -------------------------------------------------------------------------
    // GET /categories/search
    // -------------------------------------------------------------------------

    /// <summary>
    /// Récupère la liste paginée des catégories avec filtres et tri.
    /// </summary>
    /// <param name="q">Recherche textuelle sur le nom et le slug (optionnel).</param>
    /// <param name="sortBy">Critère de tri : displayOrder | name | name_desc | productCount (défaut : displayOrder).</param>
    /// <param name="page">Numéro de page base 1 (défaut : 1).</param>
    /// <param name="pageSize">Éléments par page, max 100 (défaut : 10).</param>
    /// <returns>Page de catégories avec métadonnées de pagination.</returns>
    /// <response code="200">Liste retournée avec succès.</response>
    /// <response code="400">Paramètres invalides.</response>
    [AllowAnonymous]
    [HttpGet("search")]
    [ProducesResponseType(typeof(CategoryPageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? q        = null,
        [FromQuery] string  sortBy   = "displayOrder",
        [FromQuery] int     page     = 1,
        [FromQuery] int     pageSize = 10)
    {
        if (page < 1)
            return BadRequest(new { error = "Le numéro de page doit être supérieur ou égal à 1." });

        if (pageSize is < 1 or > 100)
            return BadRequest(new { error = "La taille de page doit être comprise entre 1 et 100." });

        var validSortValues = new[] { "displayOrder", "name", "name_desc", "productCount" };
        if (!validSortValues.Contains(sortBy))
            return BadRequest(new { error = $"Valeur de tri inconnue « {sortBy} ». Valeurs acceptées : {string.Join(", ", validSortValues)}." });

        _logger.Info("GET /categories/search — q={Q}, page={Page}, sortBy={SortBy}", q, page, sortBy);

        var result = await _service.GetPagedAsync(q, sortBy, page, pageSize);
        return Ok(result);
    }

    // -------------------------------------------------------------------------
    // GET /categories/:id
    // -------------------------------------------------------------------------

    /// <summary>Récupère une catégorie par son identifiant.</summary>
    /// <param name="id">Identifiant de la catégorie.</param>
    /// <response code="200">Catégorie trouvée.</response>
    /// <response code="404">Catégorie introuvable.</response>
    [AllowAnonymous]
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var cat = await _service.GetByIdAsync(id);
        if (cat is null)
            return NotFound(new { message = $"Catégorie {id} introuvable." });

        return Ok(cat);
    }

    // -------------------------------------------------------------------------
    // POST /categories
    // -------------------------------------------------------------------------

    /// <summary>Crée une nouvelle catégorie.</summary>
    /// <param name="dto">Données de la catégorie à créer.</param>
    /// <response code="201">Catégorie créée avec succès.</response>
    /// <response code="400">Données invalides, locale inconnue ou slug déjà utilisé.</response>
    [HttpPost]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (ValidateTranslationLocales(dto.Translations) is { } localeError)
            return BadRequest(new { error = localeError });

        _logger.Info("POST /categories");

        try
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // -------------------------------------------------------------------------
    // PUT /categories/:id
    // -------------------------------------------------------------------------

    /// <summary>Met à jour une catégorie existante.</summary>
    /// <param name="id">Identifiant de la catégorie.</param>
    /// <param name="dto">Champs à mettre à jour (seuls les champs non-null sont appliqués).</param>
    /// <response code="200">Catégorie mise à jour.</response>
    /// <response code="400">Données invalides, locale inconnue ou slug déjà utilisé.</response>
    /// <response code="404">Catégorie introuvable.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (ValidateTranslationLocales(dto.Translations) is { } localeError)
            return BadRequest(new { error = localeError });

        _logger.Info("PUT /categories/{Id}", id);

        try
        {
            var updated = await _service.UpdateAsync(id, dto);
            if (updated is null)
                return NotFound(new { error = $"Catégorie {id} introuvable." });

            return Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // -------------------------------------------------------------------------
    // DELETE /categories/:id
    // -------------------------------------------------------------------------

    /// <summary>Supprime une catégorie.</summary>
    /// <param name="id">Identifiant de la catégorie.</param>
    /// <response code="204">Catégorie supprimée.</response>
    /// <response code="404">Catégorie introuvable.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        _logger.Info("DELETE /categories/{Id}", id);

        var deleted = await _service.DeleteAsync(id);
        if (!deleted)
            return NotFound(new { error = $"Catégorie {id} introuvable." });

        return NoContent();
    }
}