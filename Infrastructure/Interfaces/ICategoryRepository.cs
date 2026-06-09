namespace Infrastructure.Interfaces;

using Domain.Entities.Catalogue;

using Entities;

/// <summary>
/// Interface du dépôt de gestion des catégories.
/// Couvre les opérations CRUD et la liste paginée avec filtres.
/// </summary>
public interface ICategoryRepository
{
    /// <summary>
    /// Récupère toutes les catégories disponibles avec leurs traductions.
    /// </summary>
    /// <param name="locale">Langue des traductions à inclure.</param>
    /// <returns>La liste des catégories.</returns>
    Task<IEnumerable<Category>> GetCategoriesAsync(LocaleLang locale);

    /// <summary>
    /// Récupère les catégories avec filtres, tri et pagination.
    /// </summary>
    /// <param name="q">Recherche textuelle sur le nom et le slug (optionnel).</param>
    /// <param name="sortBy">Critère de tri : displayOrder | name | name_desc | productCount.</param>
    /// <param name="page">Numéro de page (base 1).</param>
    /// <param name="pageSize">Nombre d'éléments par page.</param>
    /// <returns>Tuple (catégories de la page, total).</returns>
    Task<(IEnumerable<Category> Items, int Total)> GetPagedAsync(
        string? q,
        string sortBy,
        int page,
        int pageSize);

    /// <summary>Récupère une catégorie par son identifiant (avec traductions).</summary>
    Task<Category?> GetByIdAsync(int id);

    /// <summary>Vérifie si un slug est déjà utilisé (optionnellement en excluant un ID).</summary>
    Task<bool> SlugExistsAsync(string slug, int? excludeId = null);

    /// <summary>Crée une nouvelle catégorie et ses traductions.</summary>
    Task<Category> CreateAsync(Category category);

    /// <summary>Met à jour une catégorie existante.</summary>
    Task<Category> UpdateAsync(Category category);

    /// <summary>Supprime une catégorie par son identifiant.</summary>
    Task DeleteAsync(int id);
}