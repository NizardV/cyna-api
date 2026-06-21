namespace Domain.Dto.User;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Réinitialisation du mot de passe à l'aide du code OTP reçu par email.
/// </summary>
public class ResetPasswordWithOtpDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>Code OTP reçu par email (6 chiffres).</summary>
    [Required]
    public string Code { get; set; } = string.Empty;

    /// <summary>Nouveau mot de passe souhaité.</summary>
    [Required, MinLength(8)]
    public string NewPassword { get; set; } = string.Empty;
}