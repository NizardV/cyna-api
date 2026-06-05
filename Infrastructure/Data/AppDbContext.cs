using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

using Entities;
using Entities.AddressAndPayment;
using Entities.AuthCodes;
using Entities.Catalogue;
using Entities.OrdersAndSubscriptions;
using Entities.PromoAndCms;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // ── Company ───────────────────────────────────────────────────────────
    public DbSet<Company> Companies => Set<Company>();

    // ── Auth ──────────────────────────────────────────────────────────────
    public DbSet<User> Users => Set<User>();
    public DbSet<EmailVerificationCode> EmailVerificationCodes => Set<EmailVerificationCode>();
    public DbSet<PasswordResetCode> PasswordResetCodes => Set<PasswordResetCode>();

    // ── Addresses & payments ──────────────────────────────────────────────
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();

    // ── Catalogue ─────────────────────────────────────────────────────────
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<CategoryTranslation> CategoryTranslations => Set<CategoryTranslation>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductTranslation> ProductTranslations => Set<ProductTranslation>();
    public DbSet<PricingPlan> PricingPlans => Set<PricingPlan>();
    public DbSet<PricingTier> PricingTiers => Set<PricingTier>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();

    // ── Cart ──────────────────────────────────────────────────────────────
    // CartItem lives in Infrastructure.Entities (not OrdersAndSubscriptions)
    public DbSet<CartItem> CartItems => Set<CartItem>();

    // ── Orders & subscriptions ────────────────────────────────────────────
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Invoice> Invoices => Set<Invoice>();

    // ── Promos ────────────────────────────────────────────────────────────
    public DbSet<PromoCode> PromoCodes => Set<PromoCode>();
    public DbSet<OrderPromoCode> OrderPromoCodes => Set<OrderPromoCode>();

    // ── CMS ───────────────────────────────────────────────────────────────
    public DbSet<CarouselSlide> CarouselSlides => Set<CarouselSlide>();
    public DbSet<CarouselSlideTranslation> CarouselSlideTranslations => Set<CarouselSlideTranslation>();
    public DbSet<SiteSetting> SiteSettings => Set<SiteSetting>();
    public DbSet<SiteSettingTranslation> SiteSettingTranslations => Set<SiteSettingTranslation>();

    // ── Support ───────────────────────────────────────────────────────────
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();
    public DbSet<ChatbotConversation> ChatbotConversations => Set<ChatbotConversation>();
    public DbSet<ChatbotMessage> ChatbotMessages => Set<ChatbotMessage>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);

        // ── Unique indexes ────────────────────────────────────────────────
        mb.Entity<User>().HasIndex(u => u.Email).IsUnique();
        mb.Entity<PaymentMethod>().HasIndex(p => p.StripePaymentMethodId).IsUnique();
        mb.Entity<Category>().HasIndex(c => c.Slug).IsUnique();
        mb.Entity<Product>().HasIndex(p => p.Slug).IsUnique();
        mb.Entity<Subscription>().HasIndex(s => s.StripeSubscriptionId).IsUnique();
        mb.Entity<Invoice>().HasIndex(i => i.InvoiceNumber).IsUnique();
        mb.Entity<PromoCode>().HasIndex(p => p.Code).IsUnique();
        mb.Entity<SiteSetting>().HasIndex(s => s.SettingKey).IsUnique();

        // ── Composite unique indexes ──────────────────────────────────────
        mb.Entity<CategoryTranslation>().HasIndex(t => new { t.CategoryId, t.Locale }).IsUnique();
        mb.Entity<ProductTranslation>().HasIndex(t => new { t.ProductId, t.Locale }).IsUnique();
        mb.Entity<CarouselSlideTranslation>().HasIndex(t => new { t.SlideId, t.Locale }).IsUnique();
        mb.Entity<SiteSettingTranslation>().HasIndex(t => new { t.SettingId, t.Locale }).IsUnique();

        // ── Optional FK ───────────────────────────────────────────────────
        mb.Entity<Order>()
            .HasOne(o => o.Subscription).WithMany(s => s.Orders)
            .HasForeignKey(o => o.SubscriptionId).IsRequired(false);
    }
}