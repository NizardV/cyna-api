namespace Infrastructure.Interfaces;

using Domain.Entities;
using Domain.Entities.AuthCodes;

using Tools;

/// <summary>
/// Interface du dépôt utilisateur.
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(int userId);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByRefreshTokenAsync(string refreshToken);

    /// <summary>Retourne tous les utilisateurs sauf celui dont l'ID est <paramref name="excludeId"/>.</summary>
    Task<IEnumerable<User>> GetAllExceptAsync(int excludeId);

    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task UpdatePasswordAsync(int userId, string newPasswordHash);

    /// <summary>Active ou désactive un compte utilisateur.</summary>
    Task SetDisabledAsync(int userId, bool isDisabled);

    /// <summary>Change le rôle d'un utilisateur.</summary>
    Task SetRoleAsync(int userId, UserRole role);

    // ── Email verification codes ──────────────────────────────────────────────

    Task AddEmailVerificationCodeAsync(EmailVerificationCode code);

    /// <summary>Retourne le code OTP email le plus récent non utilisé et non expiré.</summary>
    Task<EmailVerificationCode?> GetValidEmailVerificationCodeAsync(int userId, string code);

    Task MarkEmailVerificationCodeUsedAsync(int codeId);

    // ── Password reset codes ──────────────────────────────────────────────────

    Task AddPasswordResetCodeAsync(PasswordResetCode code);

    /// <summary>Retourne le code OTP reset le plus récent non utilisé et non expiré pour cet email.</summary>
    Task<PasswordResetCode?> GetValidPasswordResetCodeAsync(string email, string code);

    Task MarkPasswordResetCodeUsedAsync(int codeId);
}