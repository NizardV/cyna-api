namespace Api.Helpers;

using Domain.Dto.Payments;

using Infrastructure.Payments;

using Microsoft.Extensions.Options;

using Stripe;

/// <summary>Helper de test Stripe (dev only) : initialise l'API key + formate les résultats.</summary>
public sealed class StripeTestHelper
{
    public StripeTestHelper(IOptions<StripeOptions> stripeOptions)
    {
        StripeConfiguration.ApiKey = stripeOptions.Value.SecretKey;
    }

    public static object SuccessResult(PaymentIntent pi, PaymentTestRequestDto dto, string? subscriptionId)
    {
        var label = pi.Status switch
        {
            "succeeded"       => "✅ Paiement réussi",
            "requires_action" => "🔐 Authentification 3D Secure requise (à finaliser côté front)",
            _                 => $"⚠️ Statut : {pi.Status}",
        };

        return new
        {
            result          = label,
            status          = pi.Status,
            amount          = $"{dto.AmountCents / 100m:0.00} EUR",
            card            = dto.PaymentMethod,
            paymentIntentId = pi.Id,
            subscriptionId,
        };
    }

    public static object DeclineResult(StripeException ex, PaymentTestRequestDto dto, string? subscriptionId) => new
    {
        result      = "❌ Paiement refusé",
        status      = "declined",
        amount      = $"{dto.AmountCents / 100m:0.00} EUR",
        card        = dto.PaymentMethod,
        declineCode = ex.StripeError?.DeclineCode,
        errorCode   = ex.StripeError?.Code,
        message     = ex.StripeError?.Message ?? ex.Message,
        subscriptionId,
    };

    /// <summary>Extrait l'identifiant du PaymentIntent (pi_...) depuis un client secret (pi_..._secret_...).</summary>
    public static string PaymentIntentIdFrom(string? clientSecret)
    {
        if (string.IsNullOrEmpty(clientSecret)) return string.Empty;
        var idx = clientSecret.IndexOf("_secret_", StringComparison.Ordinal);
        return idx > 0 ? clientSecret[..idx] : clientSecret;
    }
}
