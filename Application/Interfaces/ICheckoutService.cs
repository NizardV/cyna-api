namespace Application.Interfaces.Services;

using Domain.Dto.Payments;

/// <summary>
/// Service d'initialisation du paiement à partir du panier de l'utilisateur.
/// Recalcule les montants côté serveur puis délègue la création du paiement à la passerelle.
/// </summary>
public interface ICheckoutService
{
    /// <summary>
    /// Initialise le paiement par abonnement à partir du panier validé de l'utilisateur.
    /// </summary>
    /// <param name="userId">L'identifiant de l'utilisateur authentifié.</param>
    /// <returns>Les client secrets et identifiants d'abonnement à confirmer côté front.</returns>
    /// <exception cref="KeyNotFoundException">Si l'utilisateur ou un plan tarifaire est introuvable.</exception>
    /// <exception cref="InvalidOperationException">Si le panier est vide ou sans ligne facturable.</exception>
    Task<PaymentInitResultDto> InitSubscriptionPaymentAsync(int userId);
}
