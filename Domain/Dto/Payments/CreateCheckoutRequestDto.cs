namespace Domain.Dto.Payments;

using Domain.Dto.Orders;

/// <summary>
/// Corps de la requête d'initialisation de paiement : l'adresse de facturation.
/// Les articles et montants sont lus/recalculés côté serveur depuis le panier.
/// </summary>
public class CreateCheckoutRequestDto
{
    /// <summary>L'adresse de facturation de l'utilisateur.</summary>
    public CreateOrderAddressDto Address { get; set; } = null!;
}
