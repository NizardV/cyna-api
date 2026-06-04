namespace Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = "user"; // user | admin | super_admin
    public bool IsEmailVerified { get; set; } = false;
    public string? TwoFactorSecret { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // FK → Company (nullable: admins have no company)
    public int? CompanyId { get; set; }
}