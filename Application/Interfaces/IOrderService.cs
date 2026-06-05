using Application.Dtos.User;

namespace Application.Interfaces.Services;

/// <summary>
/// Interface du service de gestion des commandes utilisateur.
/// Orchestre la récupération et la mise en forme de l'historique des commandes.
/// </summary>
public interface IOrderService
{
    /// <summary>
    /// Récupère l'historique complet des commandes de l'utilisateur connecté.
    /// </summary>
    /// <param name="userId">L'identifiant de l'utilisateur authentifié.</param>
    /// <returns>La liste des commandes avec leurs articles et factures.</returns>
    Task<IEnumerable<OrderSummaryDto>> GetUserOrdersAsync(int userId);
}