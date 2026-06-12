namespace Domain.Dto.Cart;

/// <summary>
/// Corps de la requête d'ajout ou de mise à jour d'un article dans le panier.
/// Au moins l'une des deux quantités doit être supérieure à zéro.
/// </summary>
public class AddCartItemRequestDto
{
    /// <summary>L'identifiant du plan tarifaire sélectionné (mensuel, annuel ou à vie).</summary>
    public int PricingPlanId { get; set; }

    /// <summary>Le nombre de licences utilisateurs souhaité (0 si non applicable).</summary>
    public int QuantityUsers { get; set; }

    /// <summary>Le nombre de licences appareils souhaité (0 si non applicable).</summary>
    public int QuantityDevices { get; set; }
}
