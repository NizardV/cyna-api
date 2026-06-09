using Domain.Entities.Catalogue;

using Tools;

namespace Infrastructure.Interfaces;

public interface IProductRepository
{
    /// <summary>
    /// Récupère les produits mis en avant (IsFeatured = true) et disponibles pour la Home.
    /// </summary>
    Task<IEnumerable<Product>> GetFeaturedProductsAsync(LocaleLang locale, int limit = 6);
}