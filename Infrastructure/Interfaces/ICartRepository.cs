namespace Infrastructure.Interfaces;

using Domain.Entities.Catalogue;
using Domain.Entities.OrdersAndSubscriptions;

/// <summary>
/// Interface du dépôt du panier d'achat.
/// Définit les opérations de lecture, d'upsert et de vidage du panier.
/// </summary>
public interface ICartRepository
{
    /// <summary>
    /// Récupère un plan tarifaire avec ses paliers de prix et son produit associé.
    /// </summary>
    /// <param name="pricingPlanId">L'identifiant du plan tarifaire.</param>
    /// <returns>Le plan tarifaire complet, ou <c>null</c> s'il est introuvable.</returns>
    Task<PricingPlan?> GetPricingPlanWithTiersAsync(int pricingPlanId);

    /// <summary>
    /// Récupère tous les articles du panier d'un utilisateur, avec leurs plans tarifaires et paliers.
    /// </summary>
    /// <param name="userId">L'identifiant de l'utilisateur.</param>
    /// <returns>La liste des articles du panier.</returns>
    Task<IEnumerable<CartItem>> GetCartItemsAsync(int userId);

    /// <summary>
    /// Insère ou met à jour un article dans le panier (upsert sur userId + pricingPlanId).
    /// </summary>
    /// <param name="userId">L'identifiant de l'utilisateur.</param>
    /// <param name="productId">L'identifiant du produit.</param>
    /// <param name="pricingPlanId">L'identifiant du plan tarifaire.</param>
    /// <param name="quantityUsers">Le nombre de licences utilisateurs.</param>
    /// <param name="quantityDevices">Le nombre de licences appareils.</param>
    /// <returns>L'article du panier créé ou mis à jour.</returns>
    Task<CartItem> UpsertCartItemAsync(int userId, int productId, int pricingPlanId, int quantityUsers, int quantityDevices);

    /// <summary>
    /// Supprime tous les articles du panier d'un utilisateur.
    /// </summary>
    /// <param name="userId">L'identifiant de l'utilisateur.</param>
    Task ClearCartAsync(int userId);
}
