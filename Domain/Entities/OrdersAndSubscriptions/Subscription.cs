namespace Domain.Entities.OrdersAndSubscriptions;

using System.ComponentModel.DataAnnotations;

using Domain.Entities.Catalogue;

using Tools;

public class Subscription
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ProductId { get; set; }
    public int PricingPlanId { get; set; }

    [MaxLength(255)]
    public string? StripeSubscriptionId { get; set; }

    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Pending;
    public DateTime CurrentPeriodStart { get; set; }
    public DateTime CurrentPeriodEnd { get; set; }
    public bool AutoRenew { get; set; } = true;

    public User User { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public PricingPlan PricingPlan { get; set; } = null!;
    public ICollection<Order> Orders { get; set; } = [];
}