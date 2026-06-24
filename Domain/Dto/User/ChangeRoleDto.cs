namespace Domain.Dto.User;

using System.ComponentModel.DataAnnotations;

using Tools;

/// <summary>
/// Payload de changement de rôle d'un utilisateur (Admin uniquement).
/// </summary>
public class ChangeRoleDto
{
    /// <summary>Nouveau rôle à assigner.</summary>
    [Required]
    public string Role { get; set; }
}