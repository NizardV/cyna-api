namespace Infrastructure.Interfaces;

using Domain.Dto.Payments;
using Domain.Entities;

/// <summary>
/// Passerelle de paiement abstraite (Stripe en réel, mock en développement/test).
/// Sélectionnée par configuration via <c>Payments:Provider</c>.
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Garantit l'existence d'un client de paiement pour l'utilisateur et renvoie son identifiant.
    /// Persiste l'identifiant sur l'utilisateur lorsqu'il vient d'être créé.
    /// </summary>
    /// <param name="user">L'utilisateur authentifié.</param>
    /// <returns>L'identifiant client (cus_...).</returns>
    Task<string> EnsureCustomerAsync(User user);

    /// <summary>
    /// Crée le(s) abonnement(s)/paiement(s) pour les lignes fournies et renvoie les éléments
    /// nécessaires à la confirmation côté front (client secrets).
    /// </summary>
    /// <param name="user">L'utilisateur authentifié.</param>
    /// <param name="request">Les lignes déjà tarifées et la devise.</param>
    /// <returns>Les client secrets et les identifiants d'abonnement créés.</returns>
    Task<PaymentInitResultDto> CreateSubscriptionPaymentAsync(User user, CreateSubscriptionPaymentRequestDto request);
}
