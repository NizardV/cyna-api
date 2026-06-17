using Infrastructure.Data;

using Microsoft.EntityFrameworkCore;

using NLog;

namespace Infrastructure.Repositories;

using Domain.Entities.Catalogue;

using Interfaces;

using Tools;

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
    public async Task<(Category? Category, IEnumerable<Product> Items, int Total)> GetCategoryCatalogAsync(
        string slug,
        string? q,
        decimal? maxPrice,
        bool available,
        int page,
        int pageSize,
        string locale)
    {
        var localeEnum = locale.ToLower() == "en" ? LocaleLang.En : LocaleLang.Fr;

        // 1. Récupération de la catégorie
        var category = await _context.Categories
            .AsNoTracking()
            .Include(c => c.Translations.Where(t => t.Locale == localeEnum))
            .FirstOrDefaultAsync(c => c.Slug == slug);

        if (category == null)
        {
            return (null, Enumerable.Empty<Product>(), 0);
        }

        // 2. Base de la requête filtre strict sur la catégorie
        var query = _context.Products
            .AsNoTracking()
            .Where(p => p.CategoryId == category.Id)
            .Include(p => p.Translations.Where(t => t.Locale == localeEnum))
            .Include(p => p.Images.OrderBy(i => i.DisplayOrder).Take(1))
            .Include(p => p.PricingPlans)
                .ThenInclude(pp => pp.PricingTiers)
            .AsQueryable();

        // 3. Filtres appliqués indépendamment de la recherche globale
        if (!string.IsNullOrWhiteSpace(q))
        {
            var qLower = q.ToLower();
            query = query.Where(p =>
                p.Translations.Any(t => t.Name.ToLower().Contains(qLower)
                                     || t.Description.ToLower().Contains(qLower)));
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(p => p.PricingPlans.Any(pp =>
                pp.BillingPeriod == BillingPeriod.Monthly &&
                pp.PricingTiers.Any(t => t.PricePerUnit <= maxPrice.Value)));
        }

        if (available)
        {
            query = query.Where(p => p.Status == ProductStatus.Available);
        }

        // 4. Algorithme de tri strict du Catalogue (Catalog Priority)
        query = query
            .OrderByDescending(p => p.Status == ProductStatus.Available)
            .ThenByDescending(p => p.IsFeatured)
            .ThenBy(p => p.DisplayOrder)
            .ThenBy(p => p.Id);

        // 5. Exécution et pagination
        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (category, items, total);
    }
}