namespace Infrastructure.Entities;

public class PasswordResetCode
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Code { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }

    public User User { get; set; } = null!;
}