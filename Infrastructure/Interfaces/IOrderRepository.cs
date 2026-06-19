namespace Infrastructure.Interfaces;

using Domain.Entities.AddressAndPayment;
using Domain.Entities.OrdersAndSubscriptions;

/// <summary>
/// Interface du dépôt des commandes utilisateur.
/// </summary>
public interface IOrderRepository
{
    /// <summary>Récupère toutes les commandes d'un utilisateur, triées par date décroissante.</summary>
    Task<IEnumerable<Order>> GetByUserIdAsync(int userId);

    /// <summary>Récupère le détail d'une commande par son identifiant.</summary>
    Task<Order?> GetByIdAsync(int orderId, int userId);

    /// <summary>
    /// Persiste une nouvelle commande complète :
    /// crée l'adresse, la commande avec ses articles, les abonnements, et vide le panier.
    /// </summary>
    Task<Order> SaveNewOrderAsync(
        Address billingAddress,
        Order order,
        IEnumerable<Subscription> subscriptions,
        int userId);
}