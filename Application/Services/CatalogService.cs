using NLog;

namespace Application.Services;

using Domain.Dto.Catalog;

using Infrastructure.Interfaces;

using Interfaces;

/// <summary>
/// Service catalogue.
/// Orchestre la recherche avec filtres, tri et pagination, et mappe vers les DTOs.
/// </summary>
public class CatalogService : ICatalogService
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    /// <summary>Taille de page par défaut pour le catalogue.</summary>
    private const int DefaultPageSize = 9;

    private readonly ICatalogRepository _catalogRepository;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="CatalogService"/>.
    /// </summary>
    /// <param name="catalogRepository">Le dépôt catalogue.</param>
    public CatalogService(ICatalogRepository catalogRepository)
    {
        _catalogRepository = catalogRepository;
    }

    /// <inheritdoc />
    public async Task<CategoryCatalogPageDto> GetCategoryCatalogAsync(
        string slug,
        string? q,
        decimal? maxPrice,
        bool available,
        int page,
        int pageSize,
        string locale)
    {

        _logger.Info(
            "Recherche catalogue par catégorie — slug={Slug}, q={Q}, page={Page}, pageSize={PageSize}, locale={Locale}",
            slug, q, page, pageSize, locale);

        page = Math.Max(1, page);
        pageSize = pageSize > 0 ? pageSize : DefaultPageSize;

        var (category, items, total) = await _catalogRepository.GetCategoryCatalogAsync(
            slug, q, maxPrice, available, page, pageSize, locale);

        if (category == null)
        {
            _logger.Warn("Catégorie introuvable pour le slug : {Slug}", slug);
            throw new KeyNotFoundException($"La catégorie avec le slug '{slug}' est introuvable.");
        }

        var totalPages = Math.Max(1, (int)Math.Ceiling((double)total / pageSize));
        var catTranslation = category.Translations.FirstOrDefault();

        return new CategoryCatalogPageDto
        {
            Total = total,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages,

            CategoryName = catTranslation?.Name ?? category.Slug,
            CategoryDescription = catTranslation?.Description,
            CategoryImageUrl = category.ImageUrl,

            Items = items.Select(p =>
            {
                var translation = p.Translations.FirstOrDefault();
                return new ProductDto
                {
                    Id = p.Id,
                    Name = translation?.Name ?? p.Slug,
                    Description = translation?.Description ?? string.Empty,
                    Status = p.Status?.ToString() ?? string.Empty,
                    ImageUrl = p.Images.FirstOrDefault()?.ImageUrl,
                    Price = p.PricingPlans
                        .SelectMany(pp => pp.PricingTiers.Select(t => new {
                            Price = t.PricePerUnit * (1 - pp.DiscountPercent / 100m)
                        }))
                        .Select(x => x.Price)
                        .DefaultIfEmpty(0)
                        .Min()
                };
            }).ToList()
        };
    }
}