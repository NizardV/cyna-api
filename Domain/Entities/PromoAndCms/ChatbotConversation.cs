namespace Domain.Entities.PromoAndCms;

using Infrastructure.Entities;
using Infrastructure.Entities.PromoAndCms;

public class ChatbotConversation
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public bool EscalatedToHuman { get; set; } = false;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
    public ICollection<ChatbotMessage> Messages { get; set; } = [];
}