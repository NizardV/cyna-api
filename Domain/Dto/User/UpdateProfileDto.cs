namespace Domain.Dto.User;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Payload de mise à jour des informations personnelles de l'utilisateur.
/// </summary>
public class UpdateProfileDto
{
    /// <summary>Nouveau prénom.</summary>
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>Nouveau nom de famille.</summary>
    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    /// <summary>Nouvelle adresse e-mail.</summary>
    [Required]
    [MaxLength(255)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}