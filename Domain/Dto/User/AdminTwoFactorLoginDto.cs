namespace Domain.Dto.User;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Connexion administrateur : email + mot de passe + code TOTP.
/// </summary>
/// <remarks>
/// <see cref="TotpCode"/> est volontairement OPTIONNEL :
/// - Si le compte n'a pas encore activé le 2FA (1ère connexion), il est ignoré
///   et la connexion réussit directement avec <c>RequiresTwoFactorSetup = true</c>.
/// - Si le compte a déjà activé le 2FA, il devient obligatoire ; son absence
///   renvoie <c>TotpRequired = true</c> sans consommer/échouer la tentative,
///   pour que le frontend puisse simplement afficher le champ et resoumettre.
/// </remarks>
public class AdminTwoFactorLoginDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    /// <summary>Code TOTP à 6 chiffres. Optionnel — voir remarks.</summary>
    public string? TotpCode { get; set; }
}