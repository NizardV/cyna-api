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
}