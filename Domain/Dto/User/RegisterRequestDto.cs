namespace Domain.Dto.User;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Corps de la requête d'inscription d'un nouvel utilisateur.
/// </summary>
public class RegisterRequestDto
{
    /// <summary>Le prénom de l'utilisateur.</summary>
    [Required]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>Le nom de famille de l'utilisateur.</summary>
    [Required]
    public string LastName { get; set; } = string.Empty;

    /// <summary>L'adresse email (doit être unique dans le système).</summary>
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>Le mot de passe en clair (minimum 6 caractères).</summary>
    [Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;
}