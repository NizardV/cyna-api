namespace Infrastructure.Entities.PromoAndCms;

using Domain.Entities;

using User = Entities.User;

public class ContactMessage
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public ContactStatus Status { get; set; } = ContactStatus.New;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
}