namespace Infrastructure.Interfaces;

using Domain.Entities.Catalogue;

public interface ISearchRepository
{
    /// <summary>
    /// Récupère les produits du catalogue avec filtres, tri et pagination côté base de données.
    /// </summary>
    /// <param name="q">Recherche textuelle sur le nom et la description (optionnel).</param>
    /// <param name="categoryIds">Liste d'identifiants de catégories pour filtrer (optionnel).</param>
    /// <param name="maxPrice">Prix mensuel maximum (optionnel).</param>
    /// <param name="available">Si true, retourne uniquement les produits disponibles.</param>
    /// <param name="sortBy">Critère de tri : relevance | price_asc | price_desc | name.</param>
    /// <param name="page">Numéro de page (base 1).</param>
    /// <param name="pageSize">Nombre d'éléments par page.</param>
    /// <param name="locale">Langue des traductions à inclure.</param>
    /// <returns>Un tuple contenant les produits de la page et le nombre total de résultats.</returns>
    Task<(IEnumerable<Product> Items, int Total)> GetProductsAsync(
        string? q,
        IEnumerable<int>? categoryIds,
        decimal? maxPrice,
        bool available,
        string sortBy,
        int page,
        int pageSize,
        string locale);
}