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

    // ── Adresses & paiements ──────────────────────────────────────────────
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();

    // ── Catalogue ─────────────────────────────────────────────────────────
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<CategoryTranslation> CategoryTranslations => Set<CategoryTranslation>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductTranslation> ProductTranslations => Set<ProductTranslation>();
    public DbSet<PricingPlan> PricingPlans => Set<PricingPlan>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();

    // ── Panier ────────────────────────────────────────────────────────────
    public DbSet<CartItem> CartItems => Set<CartItem>();

    // ── Commandes & abonnements ───────────────────────────────────────────
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Invoice> Invoices => Set<Invoice>();

    // ── Promotions ────────────────────────────────────────────────────────
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

        // ── Company ───────────────────────────────────────────────────────
        mb.Entity<Company>(e =>
        {
            e.Property(c => c.Name).IsRequired().HasMaxLength(200);
        });

        // ── Users ─────────────────────────────────────────────────────────
        mb.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.HasIndex(u => u.Role);
            e.Property(u => u.Email).IsRequired().HasMaxLength(255);
            e.Property(u => u.PasswordHash).IsRequired().HasMaxLength(255);
            e.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
            e.Property(u => u.LastName).IsRequired().HasMaxLength(100);
            e.Property(u => u.Role).IsRequired().HasMaxLength(20).HasDefaultValue("user");
            e.HasOne(u => u.Company).WithMany(c => c.Users).HasForeignKey(u => u.CompanyId).IsRequired(false);
        });

        mb.Entity<EmailVerificationCode>(e =>
        {
            e.Property(x => x.Code).IsRequired().HasMaxLength(10);
            e.HasOne(x => x.User).WithMany(u => u.EmailVerificationCodes).HasForeignKey(x => x.UserId);
        });

        mb.Entity<PasswordResetCode>(e =>
        {
            e.Property(x => x.Code).IsRequired().HasMaxLength(10);
            e.HasOne(x => x.User).WithMany(u => u.PasswordResetCodes).HasForeignKey(x => x.UserId);
        });

        // ── Addresses ─────────────────────────────────────────────────────
        mb.Entity<Address>(e =>
        {
            e.Property(a => a.FirstName).IsRequired().HasMaxLength(100);
            e.Property(a => a.LastName).IsRequired().HasMaxLength(100);
            e.Property(a => a.AddressLine1).IsRequired().HasMaxLength(255);
            e.Property(a => a.City).IsRequired().HasMaxLength(100);
            e.Property(a => a.PostalCode).IsRequired().HasMaxLength(20);
            e.Property(a => a.Country).IsRequired().HasMaxLength(2);
            e.HasOne(a => a.User).WithMany(u => u.Addresses).HasForeignKey(a => a.UserId);
        });

        mb.Entity<PaymentMethod>(e =>
        {
            e.HasIndex(p => p.StripePaymentMethodId).IsUnique();
            e.Property(p => p.StripePaymentMethodId).IsRequired().HasMaxLength(255);
            e.Property(p => p.CardLast4).IsRequired().HasMaxLength(4);
            e.HasOne(p => p.User).WithMany(u => u.PaymentMethods).HasForeignKey(p => p.UserId);
        });

        // ── Categories ────────────────────────────────────────────────────
        mb.Entity<Category>(e =>
        {
            e.HasIndex(c => c.Slug).IsUnique();
            e.Property(c => c.Slug).IsRequired().HasMaxLength(150);
        });

        mb.Entity<CategoryTranslation>(e =>
        {
            e.HasIndex(t => new { t.CategoryId, t.Locale }).IsUnique();
            e.Property(t => t.Locale).IsRequired().HasMaxLength(5);
            e.Property(t => t.Name).IsRequired().HasMaxLength(150);
            e.HasOne(t => t.Category).WithMany(c => c.Translations).HasForeignKey(t => t.CategoryId);
        });

        // ── Products ──────────────────────────────────────────────────────
        mb.Entity<Product>(e =>
        {
            e.HasIndex(p => p.Slug).IsUnique();
            e.HasIndex(p => p.CategoryId);
            e.Property(p => p.Slug).IsRequired().HasMaxLength(200);
            e.Property(p => p.Status).HasMaxLength(50);
            e.HasOne(p => p.Category).WithMany(c => c.Products).HasForeignKey(p => p.CategoryId);
        });

        mb.Entity<ProductTranslation>(e =>
        {
            e.HasIndex(t => new { t.ProductId, t.Locale }).IsUnique();
            e.Property(t => t.Locale).IsRequired().HasMaxLength(5);
            e.Property(t => t.Name).IsRequired().HasMaxLength(200);
            e.Property(t => t.Description).IsRequired();
            e.HasOne(t => t.Product).WithMany(p => p.Translations).HasForeignKey(t => t.ProductId);
        });

        mb.Entity<PricingPlan>(e =>
        {
            e.HasIndex(p => p.ProductId);
            e.Property(p => p.Name).IsRequired().HasMaxLength(100);
            e.Property(p => p.BillingPeriod).IsRequired().HasMaxLength(20);
            e.Property(p => p.Price).HasColumnType("decimal(10,2)");
            e.HasOne(p => p.Product).WithMany(pr => pr.PricingPlans).HasForeignKey(p => p.ProductId);
        });

        mb.Entity<ProductImage>(e =>
        {
            e.Property(i => i.ImageUrl).IsRequired().HasMaxLength(255);
            e.HasOne(i => i.Product).WithMany(p => p.Images).HasForeignKey(i => i.ProductId);
        });

        // ── Cart ──────────────────────────────────────────────────────────
        mb.Entity<CartItem>(e =>
        {
            e.HasOne(c => c.User).WithMany(u => u.CartItems).HasForeignKey(c => c.UserId);
            e.HasOne(c => c.Product).WithMany(p => p.CartItems).HasForeignKey(c => c.ProductId);
            e.HasOne(c => c.PricingPlan).WithMany(p => p.CartItems).HasForeignKey(c => c.PricingPlanId);
        });

        // ── Subscriptions ─────────────────────────────────────────────────
        mb.Entity<Subscription>(e =>
        {
            e.HasIndex(s => s.Status);
            e.HasIndex(s => s.CurrentPeriodEnd);
            e.HasIndex(s => s.StripeSubscriptionId).IsUnique();
            e.Property(s => s.Status).IsRequired().HasMaxLength(20).HasDefaultValue("pending");
            e.HasOne(s => s.User).WithMany(u => u.Subscriptions).HasForeignKey(s => s.UserId);
            e.HasOne(s => s.Product).WithMany(p => p.Subscriptions).HasForeignKey(s => s.ProductId);
            e.HasOne(s => s.PricingPlan).WithMany(p => p.Subscriptions).HasForeignKey(s => s.PricingPlanId);
        });

        // ── Orders ────────────────────────────────────────────────────────
        mb.Entity<Order>(e =>
        {
            e.HasIndex(o => o.UserId);
            e.HasIndex(o => o.Status);
            e.HasIndex(o => o.CreatedAt);
            e.Property(o => o.Status).IsRequired().HasMaxLength(20).HasDefaultValue("pending");
            e.Property(o => o.TotalAmount).HasColumnType("decimal(10,2)");
            e.HasOne(o => o.User).WithMany(u => u.Orders).HasForeignKey(o => o.UserId);
            e.HasOne(o => o.Subscription).WithMany(s => s.Orders).HasForeignKey(o => o.SubscriptionId).IsRequired(false);
            e.HasOne(o => o.BillingAddress).WithMany(a => a.Orders).HasForeignKey(o => o.BillingAddressId);
        });

        mb.Entity<OrderItem>(e =>
        {
            e.Property(i => i.ProductNameSnapshot).IsRequired().HasMaxLength(200);
            e.Property(i => i.PlanNameSnapshot).IsRequired().HasMaxLength(100);
            e.Property(i => i.UnitPrice).HasColumnType("decimal(10,2)");
            e.HasOne(i => i.Order).WithMany(o => o.Items).HasForeignKey(i => i.OrderId);
            e.HasOne(i => i.Product).WithMany(p => p.OrderItems).HasForeignKey(i => i.ProductId);
            e.HasOne(i => i.PricingPlan).WithMany(p => p.OrderItems).HasForeignKey(i => i.PricingPlanId);
        });

        mb.Entity<Invoice>(e =>
        {
            e.HasIndex(i => i.InvoiceNumber).IsUnique();
            e.Property(i => i.InvoiceNumber).IsRequired().HasMaxLength(50);
            e.Property(i => i.PdfUrl).IsRequired().HasMaxLength(255);
            e.HasOne(i => i.Order).WithMany(o => o.Invoices).HasForeignKey(i => i.OrderId);
        });

        // ── Promos ────────────────────────────────────────────────────────
        mb.Entity<PromoCode>(e =>
        {
            e.HasIndex(p => p.Code).IsUnique();
            e.Property(p => p.Code).IsRequired().HasMaxLength(50);
        });

        mb.Entity<OrderPromoCode>(e =>
        {
            e.Property(op => op.AppliedDiscountAmount).HasColumnType("decimal(10,2)");
            e.HasOne(op => op.Order).WithMany(o => o.PromoCodes).HasForeignKey(op => op.OrderId);
            e.HasOne(op => op.PromoCode).WithMany(p => p.Orders).HasForeignKey(op => op.PromoCodeId);
        });

        // ── CMS ───────────────────────────────────────────────────────────
        mb.Entity<CarouselSlide>(e =>
        {
            e.Property(s => s.ImageUrl).IsRequired().HasMaxLength(255);
        });

        mb.Entity<CarouselSlideTranslation>(e =>
        {
            e.HasIndex(t => new { t.SlideId, t.Locale }).IsUnique();
            e.Property(t => t.Locale).IsRequired().HasMaxLength(5);
            e.HasOne(t => t.Slide).WithMany(s => s.Translations).HasForeignKey(t => t.SlideId);
        });

        mb.Entity<SiteSetting>(e =>
        {
            e.HasIndex(s => s.SettingKey).IsUnique();
            e.Property(s => s.SettingKey).IsRequired().HasMaxLength(100);
        });

        mb.Entity<SiteSettingTranslation>(e =>
        {
            e.HasIndex(t => new { t.SettingId, t.Locale }).IsUnique();
            e.Property(t => t.Locale).IsRequired().HasMaxLength(5);
            e.Property(t => t.SettingValue).IsRequired();
            e.HasOne(t => t.Setting).WithMany(s => s.Translations).HasForeignKey(t => t.SettingId);
        });

        // ── Support ───────────────────────────────────────────────────────
        mb.Entity<ContactMessage>(e =>
        {
            e.HasIndex(c => c.Status);
            e.HasIndex(c => c.CreatedAt);
            e.Property(c => c.Email).IsRequired().HasMaxLength(255);
            e.Property(c => c.Subject).IsRequired().HasMaxLength(255);
            e.Property(c => c.Status).IsRequired().HasMaxLength(20).HasDefaultValue("new");
            e.HasOne(c => c.User).WithMany(u => u.ContactMessages).HasForeignKey(c => c.UserId).IsRequired(false);
        });

        mb.Entity<ChatbotConversation>(e =>
        {
            e.HasOne(c => c.User).WithMany(u => u.ChatbotConversations).HasForeignKey(c => c.UserId).IsRequired(false);
        });

        mb.Entity<ChatbotMessage>(e =>
        {
            e.HasIndex(m => m.ConversationId);
            e.Property(m => m.Sender).IsRequired().HasMaxLength(10);
            e.Property(m => m.Content).IsRequired();
            e.HasOne(m => m.Conversation).WithMany(c => c.Messages).HasForeignKey(m => m.ConversationId);
        });
    }
}