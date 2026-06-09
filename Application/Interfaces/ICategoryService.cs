namespace Application.Interfaces;

using Domain.Dto.Category;

/// <summary>
/// Interface du service des catégories.
/// </summary>
public interface ICategoryService
{
    /// <summary>
    /// Récupère toutes les catégories disponibles dans le catalogue.
    /// </summary>
    /// <param name="locale">Langue des traductions : fr | en (défaut : fr).</param>
    /// <returns>La liste des catégories.</returns>
    Task<IEnumerable<CategoryDto>> GetCategoriesAsync(string locale);

    /// <summary>
    /// Récupère les catégories paginées avec filtres et tri.
    /// </summary>
    Task<CategoryPageDto> GetPagedAsync(string? q, string sortBy, int page, int pageSize);

    /// <summary>Récupère une catégorie par son identifiant.</summary>
    Task<CategoryDto?> GetByIdAsync(int id);

    /// <summary>Crée une nouvelle catégorie.</summary>
    /// <returns>La catégorie créée.</returns>
    Task<CategoryDto> CreateAsync(CreateCategoryDto dto);

    /// <summary>Met à jour une catégorie existante.</summary>
    /// <returns>La catégorie mise à jour, ou null si introuvable.</returns>
    Task<CategoryDto?> UpdateAsync(int id, UpdateCategoryDto dto);

    /// <summary>
    /// Supprime une catégorie.
    /// </summary>
    /// <returns>False si la catégorie est introuvable.</returns>
    Task<bool> DeleteAsync(int id);
}