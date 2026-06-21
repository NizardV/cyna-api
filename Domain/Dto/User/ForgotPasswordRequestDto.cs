namespace Domain.Dto.User;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Demande d'envoi d'un code OTP de réinitialisation de mot de passe.
/// </summary>
public class ForgotPasswordRequestDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}