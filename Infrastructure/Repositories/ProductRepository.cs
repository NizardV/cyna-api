using Domain.Entities.Catalogue;

using Infrastructure.Data;
using Infrastructure.Interfaces;

using Microsoft.EntityFrameworkCore;

using Tools;

namespace Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Product>> GetFeaturedProductsAsync(LocaleLang locale, int limit = 6)
    {
        return await _context.Products
            .AsNoTracking()
            .Where(p => p.IsFeatured && p.Status == ProductStatus.Available)
            .Include(p => p.Translations.Where(t => t.Locale == locale))
            .Include(p => p.Images.OrderBy(i => i.DisplayOrder).Take(1)) // Seulement la 1ère image
            .Include(p => p.PricingPlans.Where(pp => pp.BillingPeriod == BillingPeriod.Monthly)) // On cherche les plans mensuels
                .ThenInclude(pp => pp.PricingTiers) // Pour trouver le prix le plus bas
            .OrderByDescending(p => p.CreatedAt) // Les plus récents en premier
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
}