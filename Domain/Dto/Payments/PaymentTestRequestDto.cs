namespace Domain.Dto.Payments;

using System.ComponentModel;

/// <summary>
/// Corps des routes de test de paiement (dev uniquement).
/// Tous les champs ont une valeur par défaut : un body vide <c>{}</c> teste un paiement réussi à 1 €.
/// </summary>
public class PaymentTestRequestDto
{
    /// <summary>Le montant en centimes (100 = 1,00 €).</summary>
    [DefaultValue(100)]
    public long AmountCents { get; set; } = 100;

    /// <summary>
    /// Le PaymentMethod de test Stripe. Exemples :
    /// pm_card_visa (succès), pm_card_chargeDeclined (refus),
    /// pm_card_chargeDeclinedInsufficientFunds (fonds insuffisants),
    /// pm_card_authenticationRequired (3D Secure), pm_card_mastercard, pm_card_amex.
    /// </summary>
    [DefaultValue("pm_card_visa")]
    public string PaymentMethod { get; set; } = "pm_card_visa";
}
