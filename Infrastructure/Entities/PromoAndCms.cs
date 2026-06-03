namespace Infrastructure.Entities;

public class PromoCode
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public int DiscountPercent { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<OrderPromoCode> Orders { get; set; } = [];
}

public class OrderPromoCode
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int PromoCodeId { get; set; }
    public decimal AppliedDiscountAmount { get; set; }

    public Order Order { get; set; } = null!;
    public PromoCode PromoCode { get; set; } = null!;
}

// ── CMS / Contenu page d'accueil ──────────────────────────────
public class CarouselSlide
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;

    public ICollection<CarouselSlideTranslation> Translations { get; set; } = [];
}

public class CarouselSlideTranslation
{
    public int Id { get; set; }
    public int SlideId { get; set; }
    public string Locale { get; set; } = string.Empty; // fr | en
    public string? Title { get; set; }
    public string? Subtitle { get; set; }
    public string? ButtonText { get; set; }

    public CarouselSlide Slide { get; set; } = null!;
}

public class SiteSetting
{
    public int Id { get; set; }
    public string SettingKey { get; set; } = string.Empty; // ex: homepage_mission_text

    public ICollection<SiteSettingTranslation> Translations { get; set; } = [];
}

public class SiteSettingTranslation
{
    public int Id { get; set; }
    public int SettingId { get; set; }
    public string Locale { get; set; } = string.Empty; // fr | en
    public string SettingValue { get; set; } = string.Empty;

    public SiteSetting Setting { get; set; } = null!;
}

// ── Support & Chatbot ─────────────────────────────────────────
public class ContactMessage
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Status { get; set; } = "new"; // new | in_progress | resolved | closed
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
}

public class ChatbotConversation
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public bool EscalatedToHuman { get; set; } = false;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
    public ICollection<ChatbotMessage> Messages { get; set; } = [];
}

public class ChatbotMessage
{
    public int Id { get; set; }
    public int ConversationId { get; set; }
    public string Sender { get; set; } = string.Empty; // user | bot | agent
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ChatbotConversation Conversation { get; set; } = null!;
}

