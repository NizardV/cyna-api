namespace Infrastructure.Entities.OrdersAndSubscriptions;

using AddressAndPayment;

using Domain.Entities;

using PromoAndCms;

using User = Entities.User;

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int? SubscriptionId { get; set; }
    public int BillingAddressId { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
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