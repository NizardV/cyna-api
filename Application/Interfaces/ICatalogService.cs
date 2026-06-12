namespace Application.Interfaces;

using Domain.Dto.Catalog;

/// <summary>
/// Interface du service catalogue.
/// Orchestre la recherche, le filtrage et la pagination des produits et catégories.
/// </summary>
public interface ICatalogService
{
    /// <summary>
    /// Recherche les produits du catalogue avec filtres, tri et pagination.
    /// </summary>
    /// <param name="q">Recherche textuelle (optionnel).</param>
    /// <param name="categoryIds">Filtre sur les identifiants de catégories (optionnel).</param>
    /// <param name="maxPrice">Prix mensuel maximum (optionnel).</param>
    /// <param name="available">Si true, retourne uniquement les produits disponibles.</param>
    /// <param name="sortBy">Critère de tri : relevance | price_asc | price_desc | name.</param>
    /// <param name="page">Numéro de page (base 1, défaut : 1).</param>
    /// <param name="pageSize">Nombre d'éléments par page (défaut : 9).</param>
    /// <param name="locale">Langue des traductions : fr | en (défaut : fr).</param>
    /// <returns>La page de résultats avec métadonnées de pagination.</returns>
    Task<CatalogPageDto> GetProductsAsync(
        string? q,
        string? categoryIds,
        decimal? maxPrice,
        bool available,
        string sortBy,
        int page,
        int pageSize,
        string locale);

    /// <summary>
    /// Récupère une catégorie et ses produits selon les règles métier strictes du catalogue (tri automatique).
    /// Mappe les données de l'en-tête de la catégorie ainsi que les produits vers un objet DTO étendu.
    /// </summary>
    /// <param name="slug">Le slug identifiant la catégorie cible.</param>
    /// <param name="q">Texte de recherche localisé dans la catégorie.</param>
    /// <param name="maxPrice">Prix unitaire maximum autorisé.</param>
    /// <param name="available">Filtre de disponibilité des produits.</param>
    /// <param name="page">Numéro de la page.</param>
    /// <param name="pageSize">Nombre d'éléments par page.</param>
    /// <param name="locale">Langue pour les traductions ("fr" ou "en").</param>
    /// <returns>Un objet <see cref="CategoryCatalogPageDto"/> contenant la bannière de la catégorie et les produits.</returns>
    /// <exception cref="KeyNotFoundException">Levée si aucune catégorie ne correspond au slug fourni.</exception>
    Task<CategoryCatalogPageDto> GetCategoryCatalogAsync(
        string slug, string? q, decimal? maxPrice, bool available, int page, int pageSize, string locale);
}