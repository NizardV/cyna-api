using Application.Interfaces.Services;

using Domain.Dto.Product;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using NLog;

using Tools;

using ILogger = NLog.ILogger;

namespace Api.Controllers;

[ApiController]
[Route("products")]
[Produces("application/json")]
public class ProductController : ControllerBase
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    // -------------------------------------------------------------------------
    // GET /products
    // -------------------------------------------------------------------------

    /// <summary>
    /// Récupère la liste complète des produits pour le back-office (tous statuts confondus).
    /// </summary>
    /// <response code="200">Liste retournée avec succès.</response>
    /// <response code="401">Utilisateur non authentifié.</response>
    /// <response code="403">Utilisateur non administrateur.</response>
    [HttpGet]
    //# todo [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(IEnumerable<ProductAdminListItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<ProductAdminListItemDto>>> GetProducts()
    {
        _logger.Info("GET /products — liste back-office");

        var products = await _productService.GetProductsForAdminAsync();
        return Ok(products);
    }

    // -------------------------------------------------------------------------
    // GET /products/{id}
    // -------------------------------------------------------------------------

    /// <summary>
    /// Récupère les détails complets d'un produit (Données isolées).
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDetailsDto>> GetProductDetails(int id, [FromQuery] LocaleLang locale = LocaleLang.Fr)
    {
        var product = await _productService.GetProductDetailsAsync(id, locale);

        if (product == null)
        {
            return NotFound(new { message = $"Le produit avec l'ID {id} n'existe pas." });
        }

        return Ok(product);
    }

    // -------------------------------------------------------------------------
    // GET /products/{id}/similar  (alias : /products/similar/{id})
    // -------------------------------------------------------------------------

    /// <summary>
    /// Récupère 6 produits similaires (même catégorie en priorité, disponibles en priorité, aléatoires).
    /// </summary>
    [HttpGet("{id:int}/similar")]
    [HttpGet("similar/{id:int}")]
    [ProducesResponseType(typeof(IEnumerable<ProductSimilarDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProductSimilarDto>>> GetSimilarProducts(int id, [FromQuery] LocaleLang locale = LocaleLang.Fr)
    {
        var similarProducts = await _productService.GetSimilarProductsAsync(id, locale);

        // Même s'il n'y a pas de produits similaires, on renvoie un tableau vide (200 OK)
        // C'est une meilleure pratique que de renvoyer une 404 pour une liste annexe.
        return Ok(similarProducts);
    }

    // -------------------------------------------------------------------------
    // GET /products/{id}/admin
    // -------------------------------------------------------------------------

    /// <summary>
    /// Récupère un produit complet pour le formulaire d'édition du back-office (les deux locales).
    /// </summary>
    /// <response code="200">Produit retourné avec succès.</response>
    /// <response code="401">Utilisateur non authentifié.</response>
    /// <response code="403">Utilisateur non administrateur.</response>
    /// <response code="404">Produit introuvable.</response>
    [HttpGet("{id:int}/admin")]
    //# todo [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ProductAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductAdminDto>> GetProductForAdmin(int id)
    {
        var product = await _productService.GetProductForAdminAsync(id);

        if (product == null)
        {
            return NotFound(new { message = $"Le produit avec l'ID {id} n'existe pas." });
        }

        return Ok(product);
    }

    // -------------------------------------------------------------------------
    // POST /products
    // -------------------------------------------------------------------------

    /// <summary>
    /// Crée un produit avec ses traductions, son image principale et ses plans tarifaires.
    /// </summary>
    /// <response code="201">Produit créé avec succès.</response>
    /// <response code="400">Données invalides (statut, catégorie, plans ou paliers).</response>
    /// <response code="401">Utilisateur non authentifié.</response>
    /// <response code="403">Utilisateur non administrateur.</response>
    [HttpPost]
    //# todo [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ProductAdminDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateProduct([FromBody] ProductUpsertRequestDto dto)
    {
        try
        {
            _logger.Info("POST /products — création du produit '{Name}'", dto.NameFr);

            var created = await _productService.CreateProductAsync(dto);
            return CreatedAtAction(nameof(GetProductForAdmin), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            _logger.Warn(ex, "Données invalides sur POST /products");
            return BadRequest(new { message = ex.Message });
        }
    }

    // -------------------------------------------------------------------------
    // PUT /products/{id}
    // -------------------------------------------------------------------------

    /// <summary>
    /// Met à jour un produit existant (remplacement complet des champs éditables).
    /// </summary>
    /// <response code="200">Produit mis à jour avec succès.</response>
    /// <response code="400">Données invalides (statut, catégorie, plans ou paliers).</response>
    /// <response code="401">Utilisateur non authentifié.</response>
    /// <response code="403">Utilisateur non administrateur.</response>
    /// <response code="404">Produit introuvable.</response>
    /// <response code="409">Un plan retiré est référencé par des commandes ou abonnements.</response>
    [HttpPut("{id:int}")]
    //# todo [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ProductAdminDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductUpsertRequestDto dto)
    {
        try
        {
            _logger.Info("PUT /products/{Id} — mise à jour", id);

            var updated = await _productService.UpdateProductAsync(id, dto);

            if (updated == null)
            {
                return NotFound(new { message = $"Le produit avec l'ID {id} n'existe pas." });
            }

            return Ok(updated);
        }
        catch (ArgumentException ex)
        {
            _logger.Warn(ex, "Données invalides sur PUT /products/{Id}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.Warn(ex, "Conflit sur PUT /products/{Id}", id);
            return Conflict(new { message = ex.Message });
        }
    }

    // -------------------------------------------------------------------------
    // DELETE /products/{id}
    // -------------------------------------------------------------------------

    /// <summary>
    /// Supprime un produit et son graphe catalogue (traductions, images, plans, paliers).
    /// </summary>
    /// <response code="204">Produit supprimé avec succès.</response>
    /// <response code="401">Utilisateur non authentifié.</response>
    /// <response code="403">Utilisateur non administrateur.</response>
    /// <response code="404">Produit introuvable.</response>
    /// <response code="409">Le produit est référencé par des commandes ou abonnements.</response>
    [HttpDelete("{id:int}")]
    //# todo [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        try
        {
            _logger.Info("DELETE /products/{Id}", id);

            await _productService.DeleteProductAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.Warn(ex, "Produit introuvable sur DELETE /products/{Id}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.Warn(ex, "Conflit sur DELETE /products/{Id}", id);
            return Conflict(new { message = ex.Message });
        }
    }
}
