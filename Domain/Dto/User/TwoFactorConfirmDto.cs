namespace Domain.Dto.User;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Confirmation de l'activation du 2FA en fournissant un premier code TOTP valide.
/// </summary>
public class TwoFactorConfirmDto
{
    [Required]
    public string TotpCode { get; set; } = string.Empty;
}