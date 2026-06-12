namespace Domain.Dto.User;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Corps de la requête de connexion.
/// </summary>
public class LoginRequestDto
{
    /// <summary>L'adresse email de l'utilisateur.</summary>
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>Le mot de passe en clair (transmis uniquement en HTTPS).</summary>
    [Required]
    public string Password { get; set; } = string.Empty;
}