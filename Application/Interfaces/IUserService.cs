namespace Application.Interfaces.Services;

using Domain.Dto.User;

/// <summary>
/// Interface du service de gestion du profil et de la sécurité utilisateur.
/// Orchestre les opérations de lecture et de mise à jour du compte.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Récupère le profil de l'utilisateur connecté.
    /// </summary>
    /// <param name="userId">L'identifiant de l'utilisateur authentifié.</param>
    /// <returns>Le profil de l'utilisateur.</returns>
    /// <exception cref="KeyNotFoundException">Si l'utilisateur n'existe pas.</exception>
    Task<UserProfileDto> GetProfileAsync(int userId);

    /// <summary>
    /// Met à jour les informations personnelles de l'utilisateur connecté.
    /// </summary>
    /// <param name="userId">L'identifiant de l'utilisateur authentifié.</param>
    /// <param name="dto">Les nouvelles valeurs du profil.</param>
    /// <returns>Le profil mis à jour.</returns>
    /// <exception cref="KeyNotFoundException">Si l'utilisateur n'existe pas.</exception>
    Task<UserProfileDto> UpdateProfileAsync(int userId, UpdateProfileDto dto);

    /// <summary>
    /// Met à jour le mot de passe de l'utilisateur connecté.
    /// Vérifie le mot de passe actuel avant toute modification.
    /// </summary>
    /// <param name="userId">L'identifiant de l'utilisateur authentifié.</param>
    /// <param name="dto">L'ancien et le nouveau mot de passe.</param>
    /// <exception cref="KeyNotFoundException">Si l'utilisateur n'existe pas.</exception>
    /// <exception cref="UnauthorizedAccessException">Si le mot de passe actuel est incorrect.</exception>
    Task UpdatePasswordAsync(int userId, UpdatePasswordDto dto);
}