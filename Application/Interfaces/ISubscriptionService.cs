namespace Application.Interfaces;

using Domain.Dto.User;

/// <summary>
/// Interface du service de gestion des abonnements utilisateur.
/// Orchestre la récupération des abonnements actifs et en cours.
/// </summary>
public interface ISubscriptionService
{
    /// <summary>
    /// Récupère tous les abonnements de l'utilisateur connecté.
    /// </summary>
    /// <param name="userId">L'identifiant de l'utilisateur authentifié.</param>
    /// <returns>La liste des abonnements avec leurs détails produit et plan.</returns>
    Task<IEnumerable<SubscriptionDto>> GetUserSubscriptionsAsync(int userId);
}