namespace Domain.Dto.User;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Payload de mise à jour du mot de passe de l'utilisateur.
/// </summary>
public class UpdatePasswordDto
{
    /// <summary>Mot de passe actuel (pour validation).</summary>
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    /// <summary>Nouveau mot de passe souhaité.</summary>
    [Required]
    [MinLength(8)]
    public string NewPassword { get; set; } = string.Empty;
}