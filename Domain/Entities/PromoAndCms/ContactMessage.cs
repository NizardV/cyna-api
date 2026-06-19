namespace Infrastructure.Entities.PromoAndCms;

using System.ComponentModel.DataAnnotations;

using Domain.Entities;

using Tools;

public class ContactMessage
{
    public int Id { get; set; }
    public int? UserId { get; set; }

    [Required, MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(255)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    public string Message { get; set; } = string.Empty;

    public ContactStatus Status { get; set; } = ContactStatus.New;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
}