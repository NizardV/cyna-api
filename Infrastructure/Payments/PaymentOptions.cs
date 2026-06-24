namespace Infrastructure.Payments;

/// <summary>
/// Options de configuration des paiements (section "Payments" de la configuration).
/// </summary>
public class PaymentOptions
{
    /// <summary>Le fournisseur de paiement : "Mock" (défaut) ou "Stripe".</summary>
    public string Provider { get; set; } = "Mock";

    /// <summary>La devise ISO 4217 en minuscules (ex : eur).</summary>
    public string Currency { get; set; } = "eur";
}

/// <summary>
/// Options de configuration Stripe (section "Stripe" de la configuration).
/// Les valeurs secrètes proviennent des secrets utilisateur (dev) ou des variables
/// d'environnement (staging/prod), jamais d'un appsettings.json versionné.
/// </summary>
public class StripeOptions
{
    public string SecretKey { get; set; } = string.Empty;
    public string PublishableKey { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
}
