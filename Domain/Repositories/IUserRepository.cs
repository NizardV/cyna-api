using Infrastructure.Entities;

namespace Domain.Repositories;

using Entities;

/// <summary>
/// Interface du dépôt utilisateur.
/// Définit les opérations de lecture/écriture sur les données utilisateur.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Récupère un utilisateur par son identifiant.
    /// </summary>
    /// <param name="userId">L'identifiant de l'utilisateur.</param>
    /// <returns>L'utilisateur correspondant, ou null s'il n'existe pas.</returns>
    Task<User?> GetByIdAsync(int userId);

    /// <summary>
    /// Met à jour le profil (prénom, nom, email) d'un utilisateur.
    /// </summary>
    /// <param name="user">L'entité utilisateur avec les nouvelles valeurs.</param>
    Task UpdateProfileAsync(User user);

    /// <summary>
    /// Met à jour le hash du mot de passe d'un utilisateur.
    /// </summary>
    /// <param name="userId">L'identifiant de l'utilisateur.</param>
    /// <param name="newPasswordHash">Le nouveau hash de mot de passe.</param>
    Task UpdatePasswordAsync(int userId, string newPasswordHash);
}