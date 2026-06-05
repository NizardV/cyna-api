namespace Infrastructure.Interfaces;

using Domain.Entities.OrdersAndSubscriptions;

/// <summary>
/// Interface du dépôt des commandes utilisateur.
/// Définit les opérations de lecture sur l'historique des commandes.
/// </summary>
public interface IOrderRepository
{
    /// <summary>
    /// Récupère toutes les commandes d'un utilisateur, triées par date décroissante.
    /// Inclut les articles, les factures et les codes promo associés.
    /// </summary>
    /// <param name="userId">L'identifiant de l'utilisateur.</param>
    /// <returns>La liste des commandes avec leurs détails.</returns>
    Task<IEnumerable<Order>> GetByUserIdAsync(int userId);

    /// <summary>
    /// Récupère le détail d'une commande par son identifiant.
    /// </summary>
    /// <param name="orderId">L'identifiant de la commande.</param>
    /// <param name="userId">L'identifiant de l'utilisateur propriétaire (sécurité).</param>
    /// <returns>La commande correspondante, ou null si elle n'existe pas / n'appartient pas à l'utilisateur.</returns>
    Task<Order?> GetByIdAsync(int orderId, int userId);
}