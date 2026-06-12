using Domain.Entities.Catalogue;

using Infrastructure.Data;
using Infrastructure.Interfaces;

using Microsoft.EntityFrameworkCore;

using Tools;

namespace Infrastructure.Repositories;

/// <summary>
/// Implémentation du dépôt produits via Entity Framework Core.
/// </summary>
public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="ProductRepository"/>.
    /// </summary>
    /// <param name="context">Le contexte de base de données.</param>
    public ProductRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Product>> GetFeaturedProductsAsync(LocaleLang locale, int limit = 6)
    {
        return await _context.Products
            .AsNoTracking()
            .Where(p => p.IsFeatured == true && p.Status.HasValue && p.Status.Value == ProductStatus.Available)
            .Include(p => p.Translations.Where(t => t.Locale == locale))
            .Include(p => p.Images.OrderBy(i => i.DisplayOrder).Take(1))
            .Include(p => p.PricingPlans.Where(pp => pp.BillingPeriod == BillingPeriod.Monthly))
                .ThenInclude(pp => pp.PricingTiers)
            .OrderByDescending(p => p.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<Product?> GetProductDetailsByIdAsync(int id, LocaleLang locale)
    {
        return await _context.Products
            .AsNoTracking()
            .Include(p => p.Translations.Where(t => t.Locale == locale))
            .Include(p => p.Category)
                .ThenInclude(c => c.Translations.Where(t => t.Locale == locale))
            .Include(p => p.Images.OrderBy(i => i.DisplayOrder))
            .Include(p => p.PricingPlans)
                .ThenInclude(pp => pp.PricingTiers.OrderBy(t => t.minQuantity))
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Product>> GetSimilarProductsAsync(int currentProductId, LocaleLang locale)
    {
        var currentCategoryId = await _context.Products
            .Where(p => p.Id == currentProductId)
            .Select(p => p.CategoryId)
            .FirstOrDefaultAsync();

        if (currentCategoryId == 0) return new List<Product>();

        var baseQuery = _context.Products
            .AsNoTracking()
            .Where(p => p.Id != currentProductId)
            .Include(p => p.Translations.Where(t => t.Locale == locale))
            .Include(p => p.Images.OrderBy(i => i.DisplayOrder).Take(1))
            .Include(p => p.PricingPlans)
            .ThenInclude(pp => pp.PricingTiers);

        // Tri côté base de données : CASE WHEN compatible SQLite et PostgreSQL
        var similarProducts = await baseQuery
            .Where(p => p.CategoryId == currentCategoryId)
            .OrderByDescending(p => p.Status == ProductStatus.Available ? 1 : 0)
            .Take(6)
            .ToListAsync();

        if (similarProducts.Count < 6)
        {
            var fallbackProducts = await baseQuery
                .Where(p => p.CategoryId != currentCategoryId)
                .OrderByDescending(p => p.Status == ProductStatus.Available ? 1 : 0)
                .Take(6 - similarProducts.Count)
                .ToListAsync();

            similarProducts.AddRange(fallbackProducts);
        }

        return similarProducts;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Product>> GetAllForAdminAsync()
    {
        return await _context.Products
            .AsNoTracking()
            .Include(p => p.Translations)
            .Include(p => p.Images.OrderBy(i => i.DisplayOrder).Take(1))
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<Product?> GetAdminDetailsByIdAsync(int id)
    {
        return await _context.Products
            .AsNoTracking()
            .Include(p => p.Translations)
            .Include(p => p.Images.OrderBy(i => i.DisplayOrder))
            .Include(p => p.PricingPlans)
                .ThenInclude(pp => pp.PricingTiers.OrderBy(t => t.minQuantity))
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    /// <inheritdoc />
    public async Task<Product?> GetEditableByIdAsync(int id)
    {
        return await _context.Products
            .Include(p => p.Translations)
            .Include(p => p.Images.OrderBy(i => i.DisplayOrder))
            .Include(p => p.PricingPlans)
                .ThenInclude(pp => pp.PricingTiers)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    /// <inheritdoc />
    public async Task<Product> AddAsync(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product;
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Product product)
    {
        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<bool> SlugExistsAsync(string slug, int? excludeProductId = null)
    {
        return await _context.Products
            .AnyAsync(p => p.Slug == slug && (excludeProductId == null || p.Id != excludeProductId));
    }

    /// <inheritdoc />
    public async Task<bool> CategoryExistsAsync(int categoryId)
    {
        return await _context.Categories.AnyAsync(c => c.Id == categoryId);
    }

    /// <inheritdoc />
    public async Task<bool> HasOrderOrSubscriptionReferencesAsync(int productId)
    {
        return await _context.OrderItems.AnyAsync(oi => oi.ProductId == productId)
            || await _context.Subscriptions.AnyAsync(s => s.ProductId == productId);
    }

    /// <inheritdoc />
    public async Task<bool> PlanHasOrderOrSubscriptionReferencesAsync(int pricingPlanId)
    {
        return await _context.OrderItems.AnyAsync(oi => oi.PricingPlanId == pricingPlanId)
            || await _context.Subscriptions.AnyAsync(s => s.PricingPlanId == pricingPlanId);
    }
}