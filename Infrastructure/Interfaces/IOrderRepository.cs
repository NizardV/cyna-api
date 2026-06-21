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

    /// <summary>Crée l'adresse de facturation et la commande (avec ses articles) en statut Pending, sans vider le panier.</summary>
    Task<Order> CreatePendingOrderAsync(Address billingAddress, Order order);

    /// <summary>Récupère une commande suivie par EF (pour mise à jour), par son identifiant.</summary>
    Task<Order?> GetTrackedByIdAsync(int orderId);

    /// <summary>Récupère une commande suivie par EF via son identifiant de PaymentIntent Stripe.</summary>
    Task<Order?> GetByStripePaymentIntentIdAsync(string paymentIntentId);

    /// <summary>Met à jour une commande.</summary>
    Task UpdateOrderAsync(Order order);

    /// <summary>Indique si une facture existe déjà pour la commande (idempotence webhook).</summary>
    Task<bool> InvoiceExistsForOrderAsync(int orderId);

    /// <summary>Ajoute une facture.</summary>
    Task AddInvoiceAsync(Invoice invoice);
}