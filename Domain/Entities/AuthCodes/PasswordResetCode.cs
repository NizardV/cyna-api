namespace Domain.Entities.AuthCodes;

using System.ComponentModel.DataAnnotations;

using Infrastructure.Entities;

/// <summary>
/// Code OTP envoyé par email pour réinitialiser le mot de passe d'un utilisateur.
/// </summary>
public class PasswordResetCode
{
    public int Id { get; set; }
    public int UserId { get; set; }

    [Required, MaxLength(10)]
    public string Code { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    /// <summary>Indique si ce code a déjà été consommé.</summary>
    public bool IsUsed { get; set; } = false;

    public User User { get; set; } = null!;
}