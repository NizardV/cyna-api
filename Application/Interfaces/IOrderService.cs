namespace Application.Interfaces;

using Domain.Dto.Orders;
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

    /// <summary>
    /// Crée une commande depuis le panier validé : valide les quantités, calcule les totaux TTC,
    /// persiste l'adresse, la commande, les abonnements et vide le panier.
    /// </summary>
    /// <param name="userId">L'identifiant de l'utilisateur authentifié.</param>
    /// <param name="dto">Les articles, l'adresse de facturation et l'identifiant de paiement Stripe.</param>
    /// <returns>Le résumé de la commande créée avec les lignes et les montants détaillés.</returns>
    /// <exception cref="ArgumentException">Si une quantité est invalide (toutes à zéro).</exception>
    /// <exception cref="InvalidOperationException">Si une quantité dépasse le seuil de commande directe.</exception>
    /// <exception cref="KeyNotFoundException">Si un plan tarifaire est introuvable.</exception>
    Task<CreateOrderResultDto> CreateOrderAsync(int userId, CreateOrderRequestDto dto);
}