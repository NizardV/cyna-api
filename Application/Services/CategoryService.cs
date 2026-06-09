using NLog;

namespace Application.Services;

using Domain.Dto.Category;
using Domain.Entities.Catalogue;

using Infrastructure.Entities;
using Infrastructure.Interfaces;

using Interfaces;

/// <summary>
/// Service des catégories.
/// Orchestre le CRUD, la pagination et le mapping vers les DTOs.
/// </summary>
public class CategoryService : ICategoryService
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    private const int DefaultPageSize = 10;

    private readonly ICategoryRepository _repo;

    public CategoryService(ICategoryRepository repo)
    {
        _repo = repo;
    }

    // -------------------------------------------------------------------------
    // Helpers de mapping
    // -------------------------------------------------------------------------

    private static CategoryDto ToDto(Category c) => new()
    {
        Id           = c.Id,
        Slug         = c.Slug,
        Name         = c.Translations.FirstOrDefault()?.Name ?? c.Slug,
        Description  = c.Translations.FirstOrDefault()?.Description,
        ImageUrl     = c.ImageUrl,
        DisplayOrder = c.DisplayOrder,
        ProductCount = c.Products?.Count ?? 0,
    };

    private static LocaleLang ParseLocale(string locale) =>
        locale.ToLower() == "en" ? LocaleLang.En : LocaleLang.Fr;

    // -------------------------------------------------------------------------
    // Lecture
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public async Task<IEnumerable<CategoryDto>> GetCategoriesAsync(string locale)
    {
        _logger.Info("Récupération des catégories pour la locale {Locale}", locale);

        var categories = await _repo.GetCategoriesAsync(locale);

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

    /// <inheritdoc />
    public async Task<CategoryPageDto> GetPagedAsync(string? q, string sortBy, int page, int pageSize)
    {
        page     = Math.Max(1, page);
        pageSize = pageSize > 0 ? pageSize : DefaultPageSize;

        var (items, total) = await _repo.GetPagedAsync(q, sortBy, page, pageSize);

        var totalPages = Math.Max(1, (int)Math.Ceiling((double)total / pageSize));

        return new CategoryPageDto
        {
            Total      = total,
            Page       = page,
            PageSize   = pageSize,
            TotalPages = totalPages,
            Items      = items.Select(ToDto).ToList(),
        };
    }

    /// <inheritdoc />
    public async Task<CategoryDto?> GetByIdAsync(int id)
    {
        var cat = await _repo.GetByIdAsync(id);
        return cat is null ? null : ToDto(cat);
    }

    // -------------------------------------------------------------------------
    // Création
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto)
    {
        // Génération du slug depuis la première traduction si absent
        var firstName = dto.Translations.FirstOrDefault()?.Name ?? "categorie";
        var slug = string.IsNullOrWhiteSpace(dto.Slug)
            ? GenerateSlug(firstName)
            : dto.Slug.Trim().ToLower();

        // Unicité du slug
        if (await _repo.SlugExistsAsync(slug))
            throw new InvalidOperationException($"Le slug « {slug} » est déjà utilisé.");

        var category = new Category
        {
            Slug         = slug,
            ImageUrl     = dto.ImageUrl,
            DisplayOrder = dto.DisplayOrder,
            Translations = dto.Translations.Select(t => new CategoryTranslation
            {
                Locale      = ParseLocale(t.Locale),
                Name        = t.Name.Trim(),
                Description = t.Description?.Trim(),
            }).ToList(),
        };

        var created = await _repo.CreateAsync(category);
        _logger.Info("Catégorie créée — Id={Id}, Slug={Slug}", created.Id, created.Slug);
        return ToDto(created);
    }

    // -------------------------------------------------------------------------
    // Mise à jour
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public async Task<CategoryDto?> UpdateAsync(int id, UpdateCategoryDto dto)
    {
        // On récupère l'entité trackée (sans AsNoTracking ici)
        var category = await _repo.GetByIdAsync(id);
        if (category is null) return null;

        // Slug
        if (dto.Slug is not null)
        {
            var slug = dto.Slug.Trim().ToLower();
            if (await _repo.SlugExistsAsync(slug, excludeId: id))
                throw new InvalidOperationException($"Le slug « {slug} » est déjà utilisé.");
            category.Slug = slug;
        }

        if (dto.ImageUrl is not null)     category.ImageUrl     = dto.ImageUrl;
        if (dto.DisplayOrder.HasValue)    category.DisplayOrder = dto.DisplayOrder.Value;

        // Translations — remplacement complet si fournies
        if (dto.Translations is not null)
        {
            category.Translations = dto.Translations.Select(t => new CategoryTranslation
            {
                CategoryId  = id,
                Locale      = ParseLocale(t.Locale),
                Name        = t.Name.Trim(),
                Description = t.Description?.Trim(),
            }).ToList();
        }

        var updated = await _repo.UpdateAsync(category);
        _logger.Info("Catégorie mise à jour — Id={Id}", id);
        return ToDto(updated);
    }

    // -------------------------------------------------------------------------
    // Suppression
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int id)
    {
        var cat = await _repo.GetByIdAsync(id);
        if (cat is null) return false;

        await _repo.DeleteAsync(id);
        _logger.Info("Catégorie supprimée — Id={Id}", id);
        return true;
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static string GenerateSlug(string name)
    {
        var slug = name.ToLower().Trim();
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[àáâãäå]", "a");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[èéêë]",   "e");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[ìíîï]",   "i");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[òóôõö]",  "o");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[ùúûü]",   "u");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[ç]",      "c");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-");
        return slug;
    }
}