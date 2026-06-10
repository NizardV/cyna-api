using Domain.Dto.Product;

using Tools;

namespace Application.Interfaces.Services;

public interface IProductService
{
    /// <summary>
    /// Récupère les détails complets et isolés d'un produit (Spécifications, images et grilles tarifaires dégressives).
    /// </summary>
    /// <param name="id">L'identifiant unique du produit à consulter.</param>
    /// <param name="locale">La langue demandée pour le filtrage des traductions (Fr ou En).</param>
    /// <returns>Le DTO des détails du produit, ou null si le produit n'existe pas ou est inactif.</returns>
    Task<ProductDetailsDto?> GetProductDetailsAsync(int id, LocaleLang locale);

    /// <summary>
    /// Récupère une liste de 6 cartes de produits similaires basés sur la même catégorie et disponibles à l'achat en priorité.
    /// </summary>
    /// <param name="currentProductId">L'identifiant du produit actuellement consulté, à exclure des recommandations.</param>
    /// <param name="locale">La langue demandée pour le filtrage des traductions (Fr ou En).</param>
    /// <returns>Une collection de DTOs allégés représentant les produits similaires, ou une liste vide.</returns>
    Task<IEnumerable<ProductSimilarDto>> GetSimilarProductsAsync(int currentProductId, LocaleLang locale);
}