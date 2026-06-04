namespace Infrastructure.Entities.PromoAndCms;

public class ChatbotMessage
{
    public int Id { get; set; }
    public int ConversationId { get; set; }
    public ChatbotSender Sender { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ChatbotConversation Conversation { get; set; } = null!;
}
