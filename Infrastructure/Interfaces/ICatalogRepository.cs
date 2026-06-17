namespace Infrastructure.Interfaces;

using Domain.Entities.Catalogue;

/// <summary>
/// Interface du dépôt du catalogue produits.
/// Définit les opérations de lecture avec filtres, tri et pagination.
/// </summary>
public interface ICatalogRepository
{
    /// <summary>
    /// Récupère les détails d'une catégorie spécifique et ses produits avec un tri métier strict (Catalog Priority).
    /// Utilisé pour la page de navigation "Catalogue par Catégorie".
    /// </summary>
    /// <param name="slug">Le slug unique identifiant la catégorie.</param>
    /// <param name="q">Texte de recherche appliqué au sein de cette catégorie.</param>
    /// <param name="maxPrice">Prix unitaire maximum autorisé.</param>
    /// <param name="available">Filtre sur les produits disponibles.</param>
    /// <param name="page">Numéro de la page (base 1).</param>
    /// <param name="pageSize">Nombre d'éléments par page.</param>
    /// <param name="locale">Langue pour les traductions ("fr" ou "en").</param>
    Task<(Category? Category, IEnumerable<Product> Items, int Total)> GetCategoryCatalogAsync(
        string slug, string? q, decimal? maxPrice, bool available, int page, int pageSize, string locale);
}