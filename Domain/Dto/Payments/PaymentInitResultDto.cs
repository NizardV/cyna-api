namespace Domain.Dto.Payments;

/// <summary>
/// Résultat d'initialisation d'un paiement : éléments nécessaires à la confirmation côté front.
/// </summary>
public class PaymentInitResultDto
{
    /// <summary>L'identifiant du client de paiement (cus_...).</summary>
    public string? CustomerId { get; set; }

    /// <summary>
    /// Un client secret par paiement à confirmer côté front
    /// (un par intervalle de facturation : une Subscription Stripe = un seul intervalle).
    /// </summary>
    public IReadOnlyList<string> ClientSecrets { get; set; } = [];

    /// <summary>Les identifiants des abonnements créés (sub_...).</summary>
    public IReadOnlyList<string> SubscriptionIds { get; set; } = [];
}
