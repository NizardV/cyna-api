namespace Application.Interfaces;

using Domain.Dto.User;
using Tools;

/// <summary>
/// Interface du service de gestion des utilisateurs.
/// </summary>
public interface IUserService
{
    // ── Profil ────────────────────────────────────────────────────────────────

    Task<UserProfileDto> GetProfileAsync(int userId);

    /// <summary>
    /// Met à jour le profil. Si l'email change, remet IsEmailVerified à false
    /// et envoie un nouveau code de vérification.
    /// </summary>
    Task<UserProfileDto> UpdateProfileAsync(int userId, UpdateProfileDto dto);

    Task UpdatePasswordAsync(int userId, UpdatePasswordDto dto);

    // ── Admin ─────────────────────────────────────────────────────────────────

    /// <summary>Retourne tous les utilisateurs sauf l'admin connecté.</summary>
    Task<IEnumerable<AdminUserDto>> GetAllUsersExceptAsync(int currentAdminId);

    /// <summary>Active ou désactive le compte d'un utilisateur.</summary>
    Task SetUserDisabledAsync(int targetUserId, bool isDisabled);

    /// <summary>Change le rôle d'un utilisateur.</summary>
    Task SetUserRoleAsync(int targetUserId, UserRole role);
}