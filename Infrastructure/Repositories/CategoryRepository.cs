using Domain.Entities.Catalogue;

using Infrastructure.Data;
using Infrastructure.Interfaces;

using Microsoft.EntityFrameworkCore;

using Tools;

namespace Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _context;

    public CategoryRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Category>> GetCategoriesAsync(LocaleLang locale)
    {
        return await _context.Categories
            .AsNoTracking() // Optimisation de lecture
            .Include(c => c.Translations.Where(t => t.Locale == locale)) // On ne charge que la bonne langue
            .OrderBy(c => c.DisplayOrder) // Tri selon l'ordre défini par l'admin
            .ToListAsync();
    }
}