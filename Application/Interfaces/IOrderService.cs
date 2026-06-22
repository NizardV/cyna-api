namespace Application.Interfaces;

using Domain.Dto.User;

/// <summary>
/// Interface du service de gestion des commandes utilisateur.
/// </summary>
public interface IOrderService
{
    /// <summary>
    /// Récupère l'historique des commandes de l'utilisateur, triées par date décroissante.
    /// </summary>
    /// <param name="userId">L'identifiant de l'utilisateur authentifié.</param>
    /// <returns>La liste des commandes avec leurs articles et factures.</returns>
    Task<IEnumerable<OrderSummaryDto>> GetUserOrdersAsync(int userId);
}
