namespace Domain.Dto.Payments;

using Tools;

/// <summary>
/// Résultat d'initialisation d'un paiement renvoyé par la passerelle.
/// Une Subscription Stripe est créée par ligne récurrente ; les lignes "à vie" donnent un PaymentIntent unique.
/// </summary>
public class PaymentInitResultDto
{
    /// <summary>L'identifiant de la commande locale créée en statut Pending.</summary>
    public int OrderId { get; set; }

    /// <summary>L'identifiant du client de paiement (cus_...).</summary>
    public string CustomerId { get; set; } = string.Empty;

    /// <summary>Les abonnements créés (un par ligne récurrente), avec leur client secret de confirmation.</summary>
    public IReadOnlyList<RecurringPaymentResultDto> Subscriptions { get; set; } = [];

    /// <summary>L'identifiant du PaymentIntent unique pour les lignes "à vie" (le cas échéant).</summary>
    public string? LifetimePaymentIntentId { get; set; }

    /// <summary>Le client secret du paiement "à vie" (le cas échéant).</summary>
    public string? LifetimeClientSecret { get; set; }

    /// <summary>L'ensemble des client secrets à confirmer côté front.</summary>
    public IReadOnlyList<string> ClientSecrets
    {
        get
        {
            var list = Subscriptions
                .Select(s => s.ClientSecret)
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();

            if (!string.IsNullOrEmpty(LifetimeClientSecret))
                list.Add(LifetimeClientSecret);

            return list;
        }
    }
}

/// <summary>
/// Résultat de création d'un abonnement récurrent côté passerelle.
/// </summary>
public class RecurringPaymentResultDto
{
    public int ProductId { get; set; }
    public int PricingPlanId { get; set; }
    public BillingPeriod BillingPeriod { get; set; }

    /// <summary>L'identifiant de l'abonnement Stripe (sub_...).</summary>
    public string StripeSubscriptionId { get; set; } = string.Empty;

    /// <summary>Le client secret de la première facture, à confirmer côté front.</summary>
    public string ClientSecret { get; set; } = string.Empty;
}
