namespace Domain.Dto.Payments;

/// <summary>
/// Réponse de l'endpoint d'initialisation de paiement : éléments à utiliser côté front.
/// </summary>
public class CheckoutPaymentResponseDto
{
    /// <summary>Le premier client secret (cas courant : panier homogène = un seul paiement à confirmer).</summary>
    public string? ClientSecret { get; set; }

    /// <summary>Tous les client secrets à confirmer (un par périodicité + éventuel paiement à vie).</summary>
    public IReadOnlyList<string> ClientSecrets { get; set; } = [];

    /// <summary>Les identifiants des abonnements Stripe créés (sub_...).</summary>
    public IReadOnlyList<string> SubscriptionIds { get; set; } = [];

    /// <summary>La clé publiable Stripe, nécessaire pour initialiser Stripe.js côté front.</summary>
    public string PublishableKey { get; set; } = string.Empty;
}
