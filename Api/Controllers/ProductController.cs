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
}