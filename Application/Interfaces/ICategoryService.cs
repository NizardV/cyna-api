namespace Application.Interfaces;

using Domain.Dto.Category;
using Infrastructure.Entities;

/// <summary>
/// Interface du service des catégories.
/// </summary>
public interface ICategoryService
{
    /// <summary>
    /// Récupère toutes les catégories disponibles dans le catalogue.
    /// </summary>
    /// <param name="locale">Locale typée (Fr | En).</param>
    Task<IEnumerable<CategoryDto>> GetCategoriesAsync(LocaleLang locale);

    /// <summary>Récupère les catégories paginées avec filtres et tri.</summary>
    Task<CategoryPageDto> GetPagedAsync(string? q, string sortBy, int page, int pageSize);

    /// <summary>Récupère une catégorie par son identifiant.</summary>
    Task<CategoryDto?> GetByIdAsync(int id);

    /// <summary>Crée une nouvelle catégorie.</summary>
    Task<CategoryDto> CreateAsync(CreateCategoryDto dto);

    /// <summary>Met à jour une catégorie existante.</summary>
    Task<CategoryDto?> UpdateAsync(int id, UpdateCategoryDto dto);

    /// <summary>Supprime une catégorie.</summary>
    Task<bool> DeleteAsync(int id);
}