namespace Application.Interfaces.Services;

using Domain.Dto.Orders;
using Domain.Dto.Payments;

/// <summary>
/// Service d'initialisation du paiement à partir du panier de l'utilisateur.
/// Recalcule les montants côté serveur, crée la commande/abonnements en statut Pending,
/// puis délègue la création du paiement à la passerelle. Le webhook confirmera le paiement.
/// </summary>
public interface ICheckoutService
{
    /// <summary>
    /// Initialise le paiement par abonnement : crée la commande Pending + les abonnements Pending,
    /// crée le paiement chez le fournisseur et renvoie les client secrets à confirmer côté front.
    /// </summary>
    /// <param name="userId">L'identifiant de l'utilisateur authentifié.</param>
    /// <param name="address">L'adresse de facturation.</param>
    /// <returns>Le résultat d'initialisation (commande, client secrets, ids d'abonnement).</returns>
    /// <exception cref="KeyNotFoundException">Si l'utilisateur ou un plan tarifaire est introuvable.</exception>
    /// <exception cref="InvalidOperationException">Si le panier est vide ou sans ligne facturable.</exception>
    Task<PaymentInitResultDto> InitSubscriptionPaymentAsync(int userId, CreateOrderAddressDto address);
}
