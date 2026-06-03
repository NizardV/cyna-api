namespace Infrastructure.Entities;

public class Category
{
    public int Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; } = 0;

    public ICollection<CategoryTranslation> Translations { get; set; } = [];
    public ICollection<Product> Products { get; set; } = [];
}

public class CategoryTranslation
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string Locale { get; set; } = string.Empty; // fr | en
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public Category Category { get; set; } = null!;
}

public class Product
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string? TechnicalSpecs { get; set; }
    public string? Status { get; set; } // available | unavailable | preview
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

public class ProductTranslation
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string Locale { get; set; } = string.Empty; // fr | en
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public Product Product { get; set; } = null!;
}

public class PricingPlan
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string BillingPeriod { get; set; } = string.Empty; // monthly | yearly | lifetime
    public string BillingUnit { get; set; } = "user"; // user | device
    public decimal Price { get; set; }
    public int DiscountPercent { get; set; } = 0;
    public bool IsActive { get; set; } = true;

    public Product Product { get; set; } = null!;
    public ICollection<CartItem> CartItems { get; set; } = [];
    public ICollection<Subscription> Subscriptions { get; set; } = [];
    public ICollection<OrderItem> OrderItems { get; set; } = [];
}

public class ProductImage
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public int DisplayOrder { get; set; } = 0;

    public Product Product { get; set; } = null!;
}