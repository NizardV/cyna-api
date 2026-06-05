using Application.Dtos.Catalog;
using Application.Interfaces.Services;
using Domain.Repositories;
using NLog;

namespace Application.Services;

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
    public async Task<CatalogPageDto> GetProductsAsync(
        string? q,
        string? categoryIds,
        decimal? maxPrice,
        bool available,
        string sortBy,
        int page,
        int pageSize,
        string locale)
    {
        _logger.Info(
            "Recherche catalogue — q={Q}, page={Page}, pageSize={PageSize}, locale={Locale}",
            q, page, pageSize, locale);

        // Normalisation des paramètres de pagination
        page     = Math.Max(1, page);
        pageSize = pageSize > 0 ? pageSize : DefaultPageSize;

        // Conversion de la chaîne categoryIds en liste d'entiers
        var catIdList = categoryIds?
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => int.TryParse(s.Trim(), out var id) ? (int?)id : null)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToList();

        var (items, total) = await _catalogRepository.GetProductsAsync(
            q, catIdList, maxPrice, available, sortBy, page, pageSize, locale);

        var totalPages = Math.Max(1, (int)Math.Ceiling((double)total / pageSize));

        return new CatalogPageDto
        {
            Total      = total,
            Page       = page,
            PageSize   = pageSize,
            TotalPages = totalPages,
            Items      = items.Select(p =>
            {
                var translation = p.Translations.FirstOrDefault();
                var catTranslation = p.Category?.Translations.FirstOrDefault();

                return new ProductDto
                {
                    Id          = p.Id,
                    Slug        = p.Slug,
                    Name        = translation?.Name ?? p.Slug,
                    Description = translation?.Description ?? string.Empty,
                    Status      = p.Status?.ToString() ?? string.Empty,
                    IsFeatured  = p.IsFeatured,
                    CategoryId  = p.CategoryId,
                    CategoryName = catTranslation?.Name ?? p.Category?.Slug ?? string.Empty,
                    ImageUrl    = p.Images.FirstOrDefault()?.ImageUrl,
                    PricingPlans = p.PricingPlans.Select(pp => new PricingPlanDto
                    {
                        Id            = pp.Id,
                        Name          = pp.Name,
                        BillingPeriod = pp.BillingPeriod.ToString(),
                        DiscountPercent = pp.DiscountPercent,
                        PricingTiers  = pp.PricingTiers.Select(t => new PricingTierDto
                        {
                            UnitType     = t.unitType.ToString(),
                            MinQuantity  = t.minQuantity,
                            MaxQuantity  = t.maxQuantity,
                            PricePerUnit = t.PricePerUnit,
                        }).ToList(),
                    }).ToList(),
                };
            }).ToList(),
        };
    }

    /// <inheritdoc />
    public async Task<IEnumerable<CategoryDto>> GetCategoriesAsync(string locale)
    {
        _logger.Info("Récupération des catégories pour la locale {Locale}", locale);

        var categories = await _catalogRepository.GetCategoriesAsync(locale);

        return categories.Select(c =>
        {
            var translation = c.Translations.FirstOrDefault();
            return new CategoryDto
            {
                Id           = c.Id,
                Slug         = c.Slug,
                Name         = translation?.Name ?? c.Slug,
                Description  = translation?.Description,
                ImageUrl     = c.ImageUrl,
                DisplayOrder = c.DisplayOrder,
            };
        });
    }
}