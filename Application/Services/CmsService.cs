using Application.Dto.Home;
using Application.Interfaces;

using Microsoft.Extensions.Logging;

using Infrastructure.Interfaces; 

using Tools;

namespace Application.Services;

/// <summary>
/// Implémentation du service CMS.
/// Agrège les données du carrousel, des paramètres de site, des catégories et des produits
/// mis en avant pour alimenter la page d'accueil.
/// </summary>
public class CmsService : ICmsService
{
    private readonly ICarouselRepository _carouselRepository;
    private readonly ILogger<CmsService> _logger;
    private readonly ISiteSettingRepository _siteSettingRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IProductRepository _productRepository;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="CmsService"/>.
    /// </summary>
    /// <param name="carouselRepository">Le dépôt des slides du carrousel.</param>
    /// <param name="logger">Le logger.</param>
    /// <param name="siteSettingRepository">Le dépôt des paramètres CMS dynamiques.</param>
    /// <param name="categoryRepository">Le dépôt des catégories.</param>
    /// <param name="productRepository">Le dépôt des produits.</param>
    public CmsService(
        ICarouselRepository carouselRepository,
        ILogger<CmsService> logger,
        ISiteSettingRepository siteSettingRepository,
        ICategoryRepository categoryRepository,
        IProductRepository productRepository)
    {
        _carouselRepository = carouselRepository;
        _logger = logger;
        _siteSettingRepository = siteSettingRepository;
        _categoryRepository = categoryRepository;
        _productRepository = productRepository;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<CarouselSlideDto>> GetHomeCarouselAsync(LocaleLang locale)
    {
        var slides = await _carouselRepository.GetActiveSlidesAsync(locale);

        if (!slides.Any())
        {
            _logger.LogWarning("Aucun slide de carrousel actif n'a été trouvé dans la base de données.");
        }

        return slides.Select(slide =>
        {
            var translation = slide.Translations.FirstOrDefault();

            return new CarouselSlideDto
            {
                Id = slide.Id,
                ImageUrl = slide.ImageUrl,
                Title = translation?.Title,
                Subtitle = translation?.Subtitle,
                ButtonText = translation?.ButtonText
            };
        });
    }

    /// <inheritdoc />
    public async Task<string?> GetHomeMissionTextAsync(LocaleLang locale)
    {
        string key = "homepage_mission_text";

        var text = await _siteSettingRepository.GetSettingValueAsync(key, locale);

        if (string.IsNullOrWhiteSpace(text))
        {
            _logger.LogWarning("Le paramètre CMS '{Key}' est introuvable ou vide pour la langue {Locale}.", key, locale);
        }

        return text;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<CategoryDto>> GetHomeCategoriesAsync(LocaleLang locale)
    {
        var categories = await _categoryRepository.GetCategoriesAsync(locale);

        if (!categories.Any())
        {
            _logger.LogWarning("Aucune catégorie n'a été trouvée dans la base de données pour la page d'accueil (Langue demandée : {Locale}).", locale);
        }

        return categories.Select(c =>
        {
            var translation = c.Translations.FirstOrDefault();

            return new CategoryDto
            {
                Id = c.Id, 
                Slug = c.Slug,
                ImageUrl = c.ImageUrl,
                Name = translation?.Name,
                Description = translation?.Description
            };
        });
    }


    /// <inheritdoc />
    public async Task<IEnumerable<ProductSummaryDto>> GetHomeTopProductsAsync(LocaleLang locale)
    {
        var products = await _productRepository.GetFeaturedProductsAsync(locale, 6);

        if (!products.Any())
        {
            _logger.LogInformation("Aucun Top Produit n'a été trouvé pour la page d'accueil (Langue demandée : {Locale}).", locale);
        }

        return products.Select(p =>
        {
            var translation = p.Translations.FirstOrDefault();
            var image = p.Images.FirstOrDefault();

            decimal? minPrice = p.PricingPlans
                .SelectMany(plan => plan.PricingTiers)
                .Select(tier => tier.PricePerUnit)
                .DefaultIfEmpty()
                .Min();

            string? shortDesc = translation?.Description;
            if (!string.IsNullOrEmpty(shortDesc) && shortDesc.Length > 100)
            {
                shortDesc = shortDesc.Substring(0, 97) + "...";
            }

            return new ProductSummaryDto
            {
                Id = p.Id,
                Slug = p.Slug,
                Name = translation?.Name,
                ShortDescription = shortDesc,
                ImageUrl = image?.ImageUrl,
                StartingPrice = minPrice == 0 ? null : minPrice
            };
        });
    }
}