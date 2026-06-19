namespace Domain.Dto.Cart;

/// <summary>
/// Résultat complet d'une opération d'ajout au panier : article mis à jour et récapitulatif financier.
/// </summary>
public class CartResultDto
{
    /// <summary>L'identifiant de l'utilisateur propriétaire du panier.</summary>
    public int CartId { get; set; }

    /// <summary>Le client secret Stripe pour initialiser le paiement côté front.</summary>
    public string StripeClientSecret { get; set; } = string.Empty;

    /// <summary>Le détail de l'article ajouté ou mis à jour.</summary>
    public CartItemResultDto Item { get; set; } = null!;

    /// <summary>Le récapitulatif financier du panier (HT, TVA, TTC).</summary>
    public CartSummaryDto CartSummary { get; set; } = null!;
}

/// <summary>
/// Détail d'un article du panier avec les quantités, prix unitaires et paliers applicables.
/// </summary>
public class CartItemResultDto
{
    /// <summary>L'identifiant de la ligne panier.</summary>
    public int Id { get; set; }

    /// <summary>L'identifiant du plan tarifaire associé.</summary>
    public int PricingPlanId { get; set; }

    /// <summary>Le nom du produit (locale française par défaut).</summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>La période de facturation : monthly | yearly | lifetime.</summary>
    public string BillingPeriod { get; set; } = string.Empty;

    /// <summary>Le nombre de licences utilisateurs.</summary>
    public int QuantityUsers { get; set; }

    /// <summary>Le nombre de licences appareils.</summary>
    public int QuantityDevices { get; set; }

    /// <summary>Le prix unitaire par utilisateur selon le palier applicable.</summary>
    public decimal UnitPriceUsers { get; set; }

    /// <summary>Le prix unitaire par appareil selon le palier applicable.</summary>
    public decimal UnitPriceDevices { get; set; }

    /// <summary>Le total HT de la ligne (UnitPriceUsers × QuantityUsers + UnitPriceDevices × QuantityDevices).</summary>
    public decimal LineTotal { get; set; }

    /// <summary>La quantité maximale d'utilisateurs commandable en ligne.</summary>
    public int MaxUsersCheckout { get; set; }

    /// <summary>La quantité maximale d'appareils commandable en ligne.</summary>
    public int MaxDevicesCheckout { get; set; }

    /// <summary>Les paliers de prix du plan, pour affichage côté front.</summary>
    public IEnumerable<CartTierDto> PricingTiers { get; set; } = [];
}

/// <summary>
/// Un palier de prix exposé dans la réponse panier.
/// </summary>
public class CartTierDto
{
    /// <summary>Le type d'unité : user | device.</summary>
    public string UnitType { get; set; } = string.Empty;

    /// <summary>La quantité minimale pour ce palier.</summary>
    public int MinQty { get; set; }

    /// <summary>La quantité maximale pour ce palier.</summary>
    public int MaxQty { get; set; }

    /// <summary>Le prix unitaire HT pour ce palier.</summary>
    public decimal UnitPrice { get; set; }
}

/// <summary>
/// Récapitulatif financier global du panier.
/// </summary>
public class CartSummaryDto
{
    /// <summary>Le total HT du panier.</summary>
    public decimal Subtotal { get; set; }

    /// <summary>Le montant de TVA (20%).</summary>
    public decimal TaxAmount { get; set; }

    /// <summary>Le total TTC du panier.</summary>
    public decimal Total { get; set; }
}
