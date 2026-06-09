using Application.Interfaces.Services;

using Domain.Dto.Product;

using Infrastructure.Interfaces;

using Microsoft.Extensions.Logging;

using Tools;

namespace Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IProductRepository productRepository, ILogger<ProductService> logger)
    {
        _productRepository = productRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<ProductDetailsDto?> GetProductDetailsAsync(int id, LocaleLang locale)
    {
        _logger.LogInformation("Récupération isolée des détails du produit Id={Id} (Locale: {Locale})", id, locale);

        var product = await _productRepository.GetProductDetailsByIdAsync(id, locale);

        if (product == null)
        {
            _logger.LogWarning("Produit Id={Id} introuvable en base.", id);
            return null;
        }

        var pTranslation = product.Translations.FirstOrDefault();
        var cTranslation = product.Category?.Translations.FirstOrDefault();

        // Mappage exclusif vers l'arbre de DTOs "Product"
        return new ProductDetailsDto
        {
            Id = product.Id,
            Slug = product.Slug,
            Name = pTranslation?.Name ?? product.Slug,
            Description = pTranslation?.Description ?? string.Empty,
            TechnicalSpecs = product.TechnicalSpecs,
            Status = product.Status?.ToString() ?? string.Empty,

            Category = new ProductCategoryDto
            {
                Id = product.Category!.Id,
                Slug = product.Category.Slug,
                Name = cTranslation?.Name ?? product.Category.Slug,
                Description = cTranslation?.Description,
                ImageUrl = product.Category.ImageUrl
            },

            Images = product.Images.Select(i => i.ImageUrl).ToList(),

            PricingPlans = product.PricingPlans.Select(pp => new ProductPricingPlanDto
            {
                Id = pp.Id,
                Name = pp.Name,
                BillingPeriod = pp.BillingPeriod.ToString(),
                DiscountPercent = pp.DiscountPercent,
                PricingTiers = pp.PricingTiers.Select(t => new ProductPricingTierDto
                {
                    UnitType = t.unitType.ToString(),
                    MinQuantity = t.minQuantity,
                    MaxQuantity = t.maxQuantity,
                    PricePerUnit = t.PricePerUnit
                }).ToList()
            }).ToList()
        };
    }
}