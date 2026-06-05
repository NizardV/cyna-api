namespace Domain.Entities.Catalogue;

using System.ComponentModel.DataAnnotations;

using Infrastructure.Entities;

using OrdersAndSubscriptions;

public class Product
{
    public int Id { get; set; }
    public int CategoryId { get; set; }

    [Required, MaxLength(200)]
    public string Slug { get; set; } = string.Empty;

    public string? TechnicalSpecs { get; set; }

    public ProductStatus? Status { get; set; }

    public bool IsFeatured { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Category Category { get; set; } = null!;
    public ICollection<ProductTranslation> Translations { get; set; } = [];
    public ICollection<PricingPlan> PricingPlans { get; set; } = [];
    public ICollection<ProductImage> Images { get; set; } = [];
    public ICollection<CartItem> CartItems { get; set; } = [];
    public ICollection<Subscription> Subscriptions { get; set; } = [];
    public ICollection<OrderItem> OrderItems { get; set; } = [];
}