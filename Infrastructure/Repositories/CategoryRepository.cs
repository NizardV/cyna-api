using Infrastructure.Data;
using Infrastructure.Entities;

using Microsoft.EntityFrameworkCore;
using NLog;

namespace Infrastructure.Repositories;

using Domain.Entities.Catalogue;

using Interfaces;

/// <summary>
/// Implémentation EF Core du dépôt des catégories.
/// </summary>
public class CategoryRepository : ICategoryRepository
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    private readonly AppDbContext _context;

    public CategoryRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Category>> GetCategoriesAsync(LocaleLang locale)
    {
        _logger.Debug("Récupération des catégories pour la locale {Locale}", locale);

        var localeEnum = locale;

        return await _context.Categories
            .AsNoTracking()
            .Include(c => c.Translations.Where(t => t.Locale == localeEnum))
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<(IEnumerable<Category> Items, int Total)> GetPagedAsync(
        string? q,
        string sortBy,
        int page,
        int pageSize)
    {
        _logger.Debug("Admin catégories — q={Q}, sortBy={SortBy}, page={Page}", q, sortBy, page);

        var query = _context.Categories
            .AsNoTracking()
            .Include(c => c.Translations)
            .Include(c => c.Products)
            .AsQueryable();

        // --- Recherche textuelle ---
        if (!string.IsNullOrWhiteSpace(q))
        {
            var qLower = q.ToLower();
            query = query.Where(c =>
                c.Slug.ToLower().Contains(qLower) ||
                c.Translations.Any(t => t.Name.ToLower().Contains(qLower) ||
                                        (t.Description != null && t.Description.ToLower().Contains(qLower))));
        }

        // --- Tri ---
        query = sortBy switch
        {
            "name"         => query.OrderBy(c => c.Translations.FirstOrDefault()!.Name),
            "name_desc"    => query.OrderByDescending(c => c.Translations.FirstOrDefault()!.Name),
            "productCount" => query.OrderByDescending(c => c.Products.Count),
            _              => query.OrderBy(c => c.DisplayOrder),
        };

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    /// <inheritdoc />
    public async Task<Category?> GetByIdAsync(int id)
    {
        return await _context.Categories
            .AsNoTracking()
            .Include(c => c.Translations)
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    /// <inheritdoc />
    public async Task<bool> SlugExistsAsync(string slug, int? excludeId = null)
    {
        return await _context.Categories
            .AnyAsync(c => c.Slug == slug && (excludeId == null || c.Id != excludeId));
    }

    /// <inheritdoc />
    public async Task<Category> CreateAsync(Category category)
    {
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        _logger.Info("Catégorie créée — Id={Id}, Slug={Slug}", category.Id, category.Slug);
        return category;
    }

    /// <inheritdoc />
    public async Task<Category> UpdateAsync(Category category)
    {
        _context.Categories.Update(category);
        await _context.SaveChangesAsync();
        _logger.Info("Catégorie mise à jour — Id={Id}", category.Id);
        return category;
    }

    /// <inheritdoc />
    public async Task DeleteAsync(int id)
    {
        var category = await _context.Categories
            .Include(c => c.Translations)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category is null) return;

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        _logger.Info("Catégorie supprimée — Id={Id}", id);
    }
}