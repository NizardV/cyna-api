namespace Domain.Dto.User;

/// <summary>
/// Profil public de l'utilisateur connecté.
/// </summary>
public class UserProfileDto
{
    /// <summary>Identifiant de l'utilisateur.</summary>
    public int Id { get; set; }

    /// <summary>Adresse e-mail.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Prénom.</summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>Nom de famille.</summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>Rôle de l'utilisateur (User, Admin, SuperAdmin).</summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>Indique si l'adresse e-mail a été vérifiée.</summary>
    public bool IsEmailVerified { get; set; }

    /// <summary>Date de création du compte.</summary>
    public DateTime CreatedAt { get; set; }
}