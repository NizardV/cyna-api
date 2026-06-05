namespace Domain.Entities.OrdersAndSubscriptions;

using System.ComponentModel.DataAnnotations;

public class Invoice
{
    public int Id { get; set; }
    public int OrderId { get; set; }

    [Required, MaxLength(50)]
    public string InvoiceNumber { get; set; } = string.Empty;

    [Required, MaxLength(255)]
    public string PdfUrl { get; set; } = string.Empty;

    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

    public Order Order { get; set; } = null!;
}