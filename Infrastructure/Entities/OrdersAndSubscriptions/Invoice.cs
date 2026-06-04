namespace Infrastructure.Entities.OrdersAndSubscriptions;

public class Invoice
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string PdfUrl { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

    public Order Order { get; set; } = null!;
}