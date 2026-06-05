namespace Infrastructure.Entities.AuthCodes;

using System.ComponentModel.DataAnnotations;

public class EmailVerificationCode
{
    public int Id { get; set; }
    public int UserId { get; set; }

    [Required, MaxLength(10)]
    public string Code { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public User User { get; set; } = null!;
}