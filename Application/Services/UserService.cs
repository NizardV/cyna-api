namespace Application.Services;

using Application.Interfaces;

using NLog;

using Domain.Dto.User;

using Infrastructure.Interfaces;

using Tools;

/// <summary>
/// Service de gestion du profil et de la sécurité utilisateur.
/// Orchestre les interactions entre le contrôleur et le dépôt utilisateur.
/// </summary>
public class UserService : IUserService
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    private readonly IUserRepository _userRepository;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="UserService"/>.
    /// </summary>
    /// <param name="userRepository">Le dépôt utilisateur.</param>
    /// <param name="passwordHasher">Le service de hachage de mot de passe.</param>
    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <inheritdoc />
    public async Task<UserProfileDto> GetProfileAsync(int userId)
    {
        _logger.Info("Récupération du profil pour l'utilisateur ID {UserId}", userId);

        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException($"Utilisateur introuvable (ID : {userId}).");

        return new UserProfileDto
        {
            Id              = user.Id,
            Email           = user.Email,
            FirstName       = user.FirstName,
            LastName        = user.LastName,
            Role            = user.Role.ToString(),
            IsEmailVerified = user.IsEmailVerified,
            CreatedAt       = user.CreatedAt,
        };
    }

    /// <inheritdoc />
    public async Task<UserProfileDto> UpdateProfileAsync(int userId, UpdateProfileDto dto)
    {
        _logger.Info("Mise à jour du profil pour l'utilisateur ID {UserId}", userId);

        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException($"Utilisateur introuvable (ID : {userId}).");

        user.FirstName = dto.FirstName;
        user.LastName  = dto.LastName;
        user.Email     = dto.Email;

        await _userRepository.UpdateAsync(user);

        _logger.Info("Profil mis à jour avec succès pour l'utilisateur ID {UserId}", userId);

        return new UserProfileDto
        {
            Id              = user.Id,
            Email           = user.Email,
            FirstName       = user.FirstName,
            LastName        = user.LastName,
            Role            = user.Role.ToString(),
            IsEmailVerified = user.IsEmailVerified,
            CreatedAt       = user.CreatedAt,
        };
    }

    /// <inheritdoc />
    public async Task UpdatePasswordAsync(int userId, UpdatePasswordDto dto)
    {
        _logger.Info("Demande de changement de mot de passe pour l'utilisateur ID {UserId}", userId);

        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException($"Utilisateur introuvable (ID : {userId}).");

        if (!dto.CurrentPassword.VerifyHashProvided(user.PasswordHash))
        {
            _logger.Warn("Mot de passe actuel incorrect pour l'utilisateur ID {UserId}", userId);
            throw new UnauthorizedAccessException("Le mot de passe actuel est incorrect.");
        }

        var newHash = dto.NewPassword.GetHash();
        await _userRepository.UpdatePasswordAsync(userId, newHash);

        _logger.Info("Mot de passe mis à jour avec succès pour l'utilisateur ID {UserId}", userId);
    }
}