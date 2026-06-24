namespace Domain.Dto.Payments;

using Tools;

/// <summary>
/// Requête d'initialisation d'un paiement par abonnement.
/// Les lignes sont déjà tarifées côté serveur (la passerelle ne refait aucun calcul de prix).
/// </summary>
public class CreateSubscriptionPaymentRequestDto
{
    /// <summary>L'identifiant de la commande locale (Pending) associée, propagé en métadonnée Stripe.</summary>
    public int OrderId { get; set; }

    /// <summary>Les lignes à facturer.</summary>
    public IReadOnlyList<PaymentLineDto> Lines { get; set; } = [];

    /// <summary>La devise ISO 4217 en minuscules (ex : eur).</summary>
    public string Currency { get; set; } = "eur";
}

/// <summary>
/// Une ligne de paiement déjà tarifée côté serveur.
/// </summary>
public class PaymentLineDto
{
    /// <summary>L'identifiant du produit (pour la métadonnée Stripe et la réconciliation webhook).</summary>
    public int ProductId { get; set; }

    /// <summary>L'identifiant du plan tarifaire (pour la métadonnée Stripe et la réconciliation webhook).</summary>
    public int PricingPlanId { get; set; }

    /// <summary>Le nom du produit (snapshot, utilisé comme libellé côté Stripe).</summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>La périodicité de facturation (mensuel, annuel, à vie).</summary>
    public BillingPeriod BillingPeriod { get; set; }

    /// <summary>Le montant TTC de la ligne pour une période, en euros.</summary>
    public decimal Amount { get; set; }

    /// <summary>La quantité (1 par défaut, le montant étant déjà agrégé sur la ligne).</summary>
    public int Quantity { get; set; } = 1;
}
