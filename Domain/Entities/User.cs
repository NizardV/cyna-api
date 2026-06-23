namespace Domain.Entities;

using System.ComponentModel.DataAnnotations;

using Domain.Entities.AddressAndPayment;
using Domain.Entities.AuthCodes;
using Domain.Entities.OrdersAndSubscriptions;
using Domain.Entities.PromoAndCms;

using Infrastructure.Entities.PromoAndCms;

using Tools;

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

    /// <summary>Identifiant du client Stripe associé (cus_...), créé au premier paiement.</summary>
    [MaxLength(255)]
    public string? StripeCustomerId { get; set; }

    /// <summary>Indique si le compte a été désactivé par un administrateur.</summary>
    public bool IsDisabled { get; set; } = false;

    /// <summary>
    /// Clé secrète TOTP (base32). Peut être renseignée alors que le 2FA n'est
    /// PAS encore actif — voir <see cref="TwoFactorEnabled"/>. Tant que
    /// l'activation n'a pas été confirmée via un premier code valide, la
    /// présence de cette clé ne doit JAMAIS bloquer la connexion standard,
    /// sous peine de verrouiller l'admin hors de son compte.
    /// </summary>
    
    [MaxLength(100)]
    public string? TwoFactorSecret { get; set; }

    /// <summary>
    /// Indique si le 2FA est réellement actif (confirmé via un premier code TOTP valide).
    /// C'est CE flag — et non la simple présence de <see cref="TwoFactorSecret"/> —
    /// qui doit être utilisé pour décider si la connexion standard doit être bloquée
    /// et si /auth/admin/login doit exiger un code TOTP.
    /// </summary>
    public bool TwoFactorEnabled { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int? CompanyId { get; set; }
    public Company? Company { get; set; }

    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }

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