namespace Infrastructure.Entities;

using Infrastructure.Entities.AddressAndPayment;
using Infrastructure.Entities.AuthCodes;
using Infrastructure.Entities.OrdersAndSubscriptions;
using Infrastructure.Entities.PromoAndCms;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User; // user | admin | super_admin
    public bool IsEmailVerified { get; set; } = false;
    public string? TwoFactorSecret { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // FK → Company (nullable: admins have no company)
    public int? CompanyId { get; set; }
    public Company? Company { get; set; }

    // Navigation — toutes les entités sont dans Domain.Entities, pas Infrastructure
    public ICollection<EmailVerificationCode> EmailVerificationCodes { get; set; } = [];
    public ICollection<PasswordResetCode> PasswordResetCodes { get; set; } = [];
    public ICollection<Address> Addresses { get; set; } = [];
    public ICollection<PaymentMethod> PaymentMethods { get; set; } = [];
    public ICollection<CartItem> CartItems { get; set; } = [];
    public ICollection<Subscription> Subscriptions { get; set; } = [];
    public ICollection<Order> Orders { get; set; } = [];
    public ICollection<ContactMessage> ContactMessages { get; set; } = [];
    public ICollection<ChatbotConversation> ChatbotConversations { get; set; } = [];
}