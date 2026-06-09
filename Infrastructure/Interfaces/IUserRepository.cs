namespace Infrastructure.Interfaces;

using Domain.Entities;

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
    /// Récupère un utilisateur par son email
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    Task<User?> GetByEmailAsync(string email);

    /// <summary>
    /// Récupère un utilisateur par son refresh token
    /// </summary>
    /// <param name="refreshToken"></param>
    /// <returns></returns>
    Task<User?> GetByRefreshTokenAsync(string refreshToken);

    /// <summary>
    /// Ajoute un nouvel utilisateur à la base de données.
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    Task AddAsync(User user);
    /// <summary>
    /// Met à jour les informations d'un utilisateur dans la base de données.
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    Task UpdateAsync(User user);

    /// <summary>
    /// Met à jour le hash du mot de passe d'un utilisateur.
    /// </summary>
    /// <param name="userId">L'identifiant de l'utilisateur.</param>
    /// <param name="newPasswordHash">Le nouveau hash de mot de passe.</param>
    Task UpdatePasswordAsync(int userId, string newPasswordHash);

}