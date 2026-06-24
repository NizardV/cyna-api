namespace Domain.Dto.User;

/// <summary>
/// Profil public de l'utilisateur connecté.
/// </summary>
public class UserProfileDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsEmailVerified { get; set; }
    public bool IsDisabled { get; set; }

    /// <summary>True si le 2FA a été configuré ET confirmé pour ce compte.</summary>
    public bool TwoFactorEnabled { get; set; }

    public DateTime CreatedAt { get; set; }
}