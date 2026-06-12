namespace Domain.Dto.Orders;

/// <summary>
/// Corps de la requête de création de commande.
/// </summary>
public class CreateOrderRequestDto
{
    /// <summary>Les articles du panier à commander.</summary>
    public IEnumerable<CreateOrderItemDto> Items { get; set; } = [];

    /// <summary>L'adresse de facturation de l'utilisateur.</summary>
    public CreateOrderAddressDto Address { get; set; } = null!;

    /// <summary>Le total TTC attendu par le client (utilisé pour vérification côté serveur).</summary>
    public decimal Total { get; set; }

    /// <summary>L'identifiant de l'intention de paiement Stripe confirmée par le front.</summary>
    public string StripePaymentIntentId { get; set; } = string.Empty;
}

/// <summary>
/// Un article de commande soumis par le client.
/// </summary>
public class CreateOrderItemDto
{
    /// <summary>L'identifiant du plan tarifaire sélectionné.</summary>
    public int PricingPlanId { get; set; }

    /// <summary>Le nom du produit (snapshot transmis par le front pour affichage rapide).</summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>La période de facturation : monthly | yearly | lifetime.</summary>
    public string BillingPeriod { get; set; } = string.Empty;

    /// <summary>Le nombre de licences utilisateurs commandées.</summary>
    public int QuantityUsers { get; set; }

    /// <summary>Le nombre de licences appareils commandées.</summary>
    public int QuantityDevices { get; set; }
}

/// <summary>
/// Adresse de facturation soumise lors de la création de commande.
/// </summary>
public class CreateOrderAddressDto
{
    /// <summary>Le prénom du destinataire de facturation.</summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>Le nom du destinataire de facturation.</summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>La première ligne d'adresse (numéro et rue).</summary>
    public string Line1 { get; set; } = string.Empty;

    /// <summary>Le code postal.</summary>
    public string PostalCode { get; set; } = string.Empty;

    /// <summary>La ville.</summary>
    public string City { get; set; } = string.Empty;

    /// <summary>Le pays (code ISO 3166-1 alpha-2 recommandé, ex : FR).</summary>
    public string Country { get; set; } = string.Empty;
}
