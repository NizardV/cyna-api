namespace Domain.Dto.User;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Confirmation de l'adresse email via un code OTP reçu par email.
/// </summary>
public class ConfirmEmailDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>Code OTP à 6 chiffres reçu par email.</summary>
    [Required]
    public string Code { get; set; } = string.Empty;
}