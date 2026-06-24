namespace Domain.Entities.AuthCodes;

using System.ComponentModel.DataAnnotations;

using Infrastructure.Entities;

/// <summary>
/// Code OTP envoyé par email pour vérifier l'adresse email d'un utilisateur.
/// Utilisé à l'inscription et lors d'un changement d'adresse email.
/// </summary>
public class EmailVerificationCode
{
    public int Id { get; set; }
    public int UserId { get; set; }

    [Required, MaxLength(10)]
    public string Code { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public bool IsUsed { get; set; } = false;

    public User User { get; set; } = null!;
}