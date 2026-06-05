namespace Domain.Repositories;

using Entities.OrdersAndSubscriptions;

/// <summary>
/// Interface du dépôt des abonnements utilisateur.
/// Définit les opérations de lecture sur les abonnements actifs.
/// </summary>
public interface ISubscriptionRepository
{
    /// <summary>
    /// Récupère tous les abonnements d'un utilisateur.
    /// Inclut le produit et le plan tarifaire associés.
    /// </summary>
    /// <param name="userId">L'identifiant de l'utilisateur.</param>
    /// <returns>La liste des abonnements.</returns>
    Task<IEnumerable<Subscription>> GetByUserIdAsync(int userId);
}