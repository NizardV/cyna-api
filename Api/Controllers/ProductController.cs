using Application.Interfaces.Services;

using Domain.Dto.Product;

using Microsoft.AspNetCore.Mvc;

using Tools;

namespace Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Récupère les détails complets d'un produit (Données isolées).
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDetailsDto>> GetProductDetails(int id, [FromQuery] LocaleLang locale = LocaleLang.Fr)
    {
        var product = await _productService.GetProductDetailsAsync(id, locale);

        if (product == null)
        {
            return NotFound(new { message = $"Le produit avec l'ID {id} n'existe pas." });
        }

        return Ok(product);
    }

    /// <summary>
    /// Récupère 6 produits similaires (même catégorie en priorité, disponibles en priorité, aléatoires).
    /// </summary>
    [HttpGet("{id}/similar")]
    public async Task<ActionResult<IEnumerable<ProductSimilarDto>>> GetSimilarProducts(int id, [FromQuery] LocaleLang locale = LocaleLang.Fr)
    {
        var similarProducts = await _productService.GetSimilarProductsAsync(id, locale);

        // Même s'il n'y a pas de produits similaires, on renvoie un tableau vide (200 OK)
        // C'est une meilleure pratique que de renvoyer une 404 pour une liste annexe.
        return Ok(similarProducts);
    }
}