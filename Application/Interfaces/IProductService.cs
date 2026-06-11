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

    /// <summary>
    /// Récupère la liste complète des produits pour le back-office (toutes les fiches, tous statuts confondus).
    /// </summary>
    Task<IEnumerable<ProductAdminListItemDto>> GetProductsForAdminAsync();

    /// <summary>
    /// Récupère un produit complet pour le formulaire d'édition du back-office (les deux locales).
    /// </summary>
    /// <param name="id">L'identifiant du produit.</param>
    /// <returns>Le DTO admin, ou null si le produit n'existe pas.</returns>
    Task<ProductAdminDto?> GetProductForAdminAsync(int id);

    /// <summary>
    /// Crée un produit avec ses traductions, son image principale et ses plans tarifaires.
    /// Le slug est généré automatiquement à partir du nom français.
    /// </summary>
    /// <param name="dto">Le contenu du produit à créer.</param>
    /// <returns>Le produit créé au format admin.</returns>
    /// <exception cref="ArgumentException">Si la catégorie, le statut ou les plans sont invalides.</exception>
    Task<ProductAdminDto> CreateProductAsync(ProductUpsertRequestDto dto);

    /// <summary>
    /// Met à jour un produit existant (remplacement complet des champs éditables).
    /// Les plans tarifaires sont rapprochés par période de facturation ; un plan retiré
    /// n'est supprimé que s'il n'est référencé par aucune commande ni aucun abonnement.
    /// </summary>
    /// <param name="id">L'identifiant du produit à mettre à jour.</param>
    /// <param name="dto">Le nouveau contenu du produit.</param>
    /// <returns>Le produit mis à jour au format admin, ou null si le produit n'existe pas.</returns>
    /// <exception cref="ArgumentException">Si la catégorie, le statut ou les plans sont invalides.</exception>
    /// <exception cref="InvalidOperationException">Si un plan retiré est référencé par des commandes ou abonnements.</exception>
    Task<ProductAdminDto?> UpdateProductAsync(int id, ProductUpsertRequestDto dto);

    /// <summary>
    /// Supprime un produit et son graphe catalogue (traductions, images, plans, paliers).
    /// </summary>
    /// <param name="id">L'identifiant du produit à supprimer.</param>
    /// <exception cref="KeyNotFoundException">Si le produit n'existe pas.</exception>
    /// <exception cref="InvalidOperationException">Si le produit est référencé par des commandes ou abonnements.</exception>
    Task DeleteProductAsync(int id);
}