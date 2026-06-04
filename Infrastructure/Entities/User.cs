namespace Infrastructure.Entities;

using System.ComponentModel.DataAnnotations;
using Infrastructure.Entities.AddressAndPayment;
using Infrastructure.Entities.AuthCodes;
using Infrastructure.Entities.OrdersAndSubscriptions;
using Infrastructure.Entities.PromoAndCms;

public class User
{
    public int Id { get; set; }

    [Required, MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    public UserRole Role { get; set; } = UserRole.User;
    public bool IsEmailVerified { get; set; } = false;

    [MaxLength(100)]
    public string? TwoFactorSecret { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int? CompanyId { get; set; }
    public Company? Company { get; set; }

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