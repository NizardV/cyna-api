namespace Domain.Dto.User;

/// <summary>
/// Résumé d'une commande dans l'historique utilisateur.
/// </summary>
public class OrderSummaryDto
{
    /// <summary>Identifiant de la commande.</summary>
    public int Id { get; set; }

    /// <summary>Statut de la commande (Pending, Paid, Cancelled, …).</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Montant total de la commande.</summary>
    public decimal TotalAmount { get; set; }

    /// <summary>Date de création de la commande.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>URL du PDF de la facture associée, si disponible.</summary>
    public string? InvoiceUrl { get; set; }

    /// <summary>Articles inclus dans la commande.</summary>
    public IEnumerable<OrderItemDto> Items { get; set; } = [];
}