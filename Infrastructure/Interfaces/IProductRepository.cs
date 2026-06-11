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

    /// <summary>
    /// Récupère tous les produits pour le back-office (toutes les traductions et la première image).
    /// </summary>
    Task<IEnumerable<Product>> GetAllForAdminAsync();

    /// <summary>
    /// Récupère un produit complet (toutes locales, images, plans et paliers) sans suivi, pour la lecture back-office.
    /// </summary>
    Task<Product?> GetAdminDetailsByIdAsync(int id);

    /// <summary>
    /// Récupère un produit complet avec suivi des modifications, pour une mise à jour ou une suppression.
    /// </summary>
    Task<Product?> GetEditableByIdAsync(int id);

    /// <summary>
    /// Ajoute un nouveau produit (avec son graphe traductions/images/plans) et persiste immédiatement.
    /// </summary>
    Task<Product> AddAsync(Product product);

    /// <summary>
    /// Supprime un produit ; les traductions, images, plans et paliers suivent en cascade.
    /// </summary>
    Task DeleteAsync(Product product);

    /// <summary>
    /// Persiste les modifications en attente sur les entités suivies.
    /// </summary>
    Task SaveChangesAsync();

    /// <summary>
    /// Indique si un slug est déjà utilisé par un autre produit que celui exclu.
    /// </summary>
    Task<bool> SlugExistsAsync(string slug, int? excludeProductId = null);

    /// <summary>
    /// Indique si la catégorie existe.
    /// </summary>
    Task<bool> CategoryExistsAsync(int categoryId);

    /// <summary>
    /// Indique si le produit est référencé par des lignes de commande ou des abonnements (suppression interdite).
    /// </summary>
    Task<bool> HasOrderOrSubscriptionReferencesAsync(int productId);

    /// <summary>
    /// Indique si un plan tarifaire est référencé par des lignes de commande ou des abonnements.
    /// </summary>
    Task<bool> PlanHasOrderOrSubscriptionReferencesAsync(int pricingPlanId);
}