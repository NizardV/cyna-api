namespace Domain.Dto.User;

/// <summary>
/// Représentation d'un utilisateur dans la liste d'administration.
/// </summary>
public class AdminUserDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsEmailVerified { get; set; }
    public bool IsDisabled { get; set; }
    public bool HasTwoFactor { get; set; }
    public DateTime CreatedAt { get; set; }
}