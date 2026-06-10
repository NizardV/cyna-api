using Domain.Entities.Catalogue;

using Tools;

namespace Infrastructure.Interfaces;

public interface IProductRepository
{
    /// <summary>
    /// Récupère les produits mis en avant (IsFeatured = true) et disponibles pour la Home.
    /// </summary>
    Task<IEnumerable<Product>> GetFeaturedProductsAsync(LocaleLang locale, int limit = 6);

    /// <summary>
    /// Récupère les détails complets d'un produit par son ID.
    /// </summary>
    Task<Product?> GetProductDetailsByIdAsync(int id, LocaleLang locale);

    /// <summary>
    /// Récupère une liste de services SaaS similaires (Même catégorie et disponibles en priorité, triés aléatoirement).
    /// </summary>
    Task<IEnumerable<Product>> GetSimilarProductsAsync(int currentProductId, LocaleLang locale);

}