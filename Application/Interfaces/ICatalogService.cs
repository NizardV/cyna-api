namespace Application.Interfaces;

using Domain.Dto.Catalog;

/// <summary>
/// Interface du service catalogue.
/// Orchestre la recherche, le filtrage et la pagination des produits et catégories.
/// </summary>
public interface ICatalogService
{
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