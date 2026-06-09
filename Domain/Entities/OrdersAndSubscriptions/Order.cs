namespace Domain.Entities.OrdersAndSubscriptions;

using System.ComponentModel.DataAnnotations.Schema;

using Domain.Entities.AddressAndPayment;

using Infrastructure.Entities.PromoAndCms;

using PromoAndCms;

using Tools;

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int? SubscriptionId { get; set; }
    public int BillingAddressId { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    [Column(TypeName = "decimal(10,2)")]
    public decimal TotalAmount { get; set; }

    public string? StripePaymentIntentId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public Subscription? Subscription { get; set; }
    public Address BillingAddress { get; set; } = null!;
    public ICollection<OrderItem> Items { get; set; } = [];
    public ICollection<Invoice> Invoices { get; set; } = [];
    public ICollection<OrderPromoCode> PromoCodes { get; set; } = [];
}