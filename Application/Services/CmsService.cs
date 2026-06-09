using Application.Dto.Home;
using Application.Interfaces;

using Microsoft.Extensions.Logging;

using Infrastructure.Interfaces; 

using Tools;

namespace Application.Services;

/// <inheritdoc />
public class CmsService : ICmsService
{
    private readonly ICarouselRepository _carouselRepository;
    private readonly ILogger<CmsService> _logger;
    private readonly ISiteSettingRepository _siteSettingRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IProductRepository _productRepository;


    public CmsService(ICarouselRepository carouselRepository, ILogger<CmsService> logger, ISiteSettingRepository siteSettingRepository, ICategoryRepository categoryRepository, IProductRepository productRepository)
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
        // La clé exacte définie dans ta base de données
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

            // Logique Métier : Trouver le prix le plus bas parmi les Tiers du plan Mensuel
            decimal? minPrice = p.PricingPlans
                .SelectMany(plan => plan.PricingTiers)
                .Select(tier => tier.PricePerUnit)
                .DefaultIfEmpty()
                .Min();

            // Logique Métier : Créer une description courte (max 100 caractères)
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
                StartingPrice = minPrice == 0 ? null : minPrice // Si pas de prix, on renvoie null
            };
        });
    }
}