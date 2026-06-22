namespace Application.Interfaces.Services;

/// <summary>
/// Service de traitement des webhooks du fournisseur de paiement (Stripe).
/// Vérifie la signature et applique les effets en base (confirmation des commandes/abonnements).
/// </summary>
public interface IPaymentWebhookService
{
    /// <summary>
    /// Vérifie la signature et traite l'événement reçu.
    /// </summary>
    /// <param name="json">Le corps brut de la requête webhook.</param>
    /// <param name="signatureHeader">La valeur de l'en-tête <c>Stripe-Signature</c>.</param>
    /// <exception cref="Stripe.StripeException">Si la signature est invalide.</exception>
    Task HandleEventAsync(string json, string signatureHeader);
}
