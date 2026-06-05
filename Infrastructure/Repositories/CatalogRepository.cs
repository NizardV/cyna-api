using Infrastructure.Data;
using Infrastructure.Entities;

using Microsoft.EntityFrameworkCore;
using NLog;

namespace Infrastructure.Repositories;

using Domain.Entities.Catalogue;

using Interfaces;

/// <summary>
/// Implémentation du dépôt catalogue via Entity Framework Core.
/// Gère le filtrage, le tri et la pagination côté base de données.
/// </summary>
public class CatalogRepository : ICatalogRepository
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    private readonly AppDbContext _context;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="CatalogRepository"/>.
    /// </summary>
    /// <param name="context">Le contexte de base de données.</param>
    public CatalogRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<(IEnumerable<Product> Items, int Total)> GetProductsAsync(
        string? q,
        IEnumerable<int>? categoryIds,
        decimal? maxPrice,
        bool available,
        string sortBy,
        int page,
        int pageSize,
        string locale)
    {
        _logger.Debug(
            "Recherche catalogue — q={Q}, catégories={CatIds}, maxPrice={MaxPrice}, page={Page}",
            q, categoryIds, maxPrice, page);

        // Résolution de la locale en enum
        var localeEnum = locale.ToLower() == "en" ? LocaleLang.En : LocaleLang.Fr;

        var query = _context.Products
            .AsNoTracking()
            .Include(p => p.Translations.Where(t => t.Locale == localeEnum))
            .Include(p => p.Category)
                .ThenInclude(c => c.Translations.Where(t => t.Locale == localeEnum))
            .Include(p => p.Images.OrderBy(i => i.DisplayOrder).Take(1))
            .Include(p => p.PricingPlans)
                .ThenInclude(pp => pp.PricingTiers)
            .AsQueryable();

        // --- Filtrage ---
        if (!string.IsNullOrWhiteSpace(q))
        {
            var qLower = q.ToLower();
            query = query.Where(p =>
                p.Translations.Any(t => t.Name.ToLower().Contains(qLower)
                                     || t.Description.ToLower().Contains(qLower)));
        }

        var catList = categoryIds?.ToList();
        if (catList is { Count: > 0 })
        {
            query = query.Where(p => catList.Contains(p.CategoryId));
        }

        if (maxPrice.HasValue)
        {
            // Filtre sur le prix unitaire minimum parmi les paliers mensuels
            query = query.Where(p =>
                p.PricingPlans.Any(pp =>
                    pp.BillingPeriod == BillingPeriod.Monthly &&
                    pp.PricingTiers.Any(t => t.PricePerUnit <= maxPrice.Value)));
        }

        if (available)
        {
            query = query.Where(p => p.Status == ProductStatus.Available);
        }

        // --- Tri ---
        query = sortBy switch
        {
            "price_asc" => query
                .OrderByDescending(p => p.IsFeatured)
                .ThenBy(p =>
                    p.PricingPlans
                        .Where(pp => pp.BillingPeriod == BillingPeriod.Monthly)
                        .SelectMany(pp => pp.PricingTiers)
                        .Min(t => (decimal?)t.PricePerUnit) ?? decimal.MaxValue),
            "price_desc" => query
                .OrderByDescending(p => p.IsFeatured)
                .ThenByDescending(p =>
                    p.PricingPlans
                        .Where(pp => pp.BillingPeriod == BillingPeriod.Monthly)
                        .SelectMany(pp => pp.PricingTiers)
                        .Min(t => (decimal?)t.PricePerUnit) ?? decimal.Zero),
            "name" => query
                .OrderByDescending(p => p.IsFeatured)
                .ThenBy(p => p.Translations.First().Name),
            _ => query
                .OrderByDescending(p => p.IsFeatured)
                .ThenBy(p => p.Id),
        };

        // --- Pagination ---
        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Category>> GetCategoriesAsync(string locale)
    {
        _logger.Debug("Récupération des catégories pour la locale {Locale}", locale);

        var localeEnum = locale.ToLower() == "en" ? LocaleLang.En : LocaleLang.Fr;

        return await _context.Categories
            .AsNoTracking()
            .Include(c => c.Translations.Where(t => t.Locale == localeEnum))
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();
    }
}