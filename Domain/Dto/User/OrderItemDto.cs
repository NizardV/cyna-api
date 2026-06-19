namespace Domain.Dto.User;

/// <summary>
/// Article individuel d'une commande.
/// </summary>
public class OrderItemDto
{
    /// <summary>Identifiant de l'article.</summary>
    public int Id { get; set; }

    /// <summary>Nom du produit au moment de la commande (snapshot).</summary>
    public string ProductNameSnapshot { get; set; } = string.Empty;

    /// <summary>Nom du plan tarifaire au moment de la commande (snapshot).</summary>
    public string PlanNameSnapshot { get; set; } = string.Empty;

    /// <summary>Nombre d'utilisateurs commandés.</summary>
    public int QuantityUsers { get; set; }

    /// <summary>Nombre d'appareils commandés.</summary>
    public int QuantityDevices { get; set; }
}