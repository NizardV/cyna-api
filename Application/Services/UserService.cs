namespace Application.Services;

using Application.Interfaces;

using NLog;

using Domain.Dto.User;

using Domain.Entities;

using Infrastructure.Interfaces;

using Tools;

/// <summary>
/// Service de gestion du profil et de la sécurité utilisateur.
/// </summary>
public class UserService : IUserService
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    private readonly IUserRepository _userRepository;
    private readonly AuthService     _authService;

    public UserService(IUserRepository userRepository, AuthService authService)
    {
        _userRepository = userRepository;
        _authService    = authService;
    }

    // ── Profile ───────────────────────────────────────────────────────────────

    public async Task<UserProfileDto> GetProfileAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException($"Utilisateur introuvable (ID : {userId}).");

        return ToProfileDto(user);
    }

    public async Task<UserProfileDto> UpdateProfileAsync(int userId, UpdateProfileDto dto)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException($"Utilisateur introuvable (ID : {userId}).");

        var emailChanged = !string.Equals(user.Email, dto.Email, StringComparison.OrdinalIgnoreCase);

        user.FirstName = dto.FirstName;
        user.LastName  = dto.LastName;
        user.Email     = dto.Email;

        if (emailChanged)
        {
            user.IsEmailVerified = false;
            _logger.Info("Email changé pour l'utilisateur ID {UserId} — vérification requise.", userId);
        }

        await _userRepository.UpdateAsync(user);

        if (emailChanged)
        {
            await _authService.SendEmailVerificationOtpInternalAsync(user);
        }

        _logger.Info("Profil mis à jour pour l'utilisateur ID {UserId}", userId);
        return ToProfileDto(user);
    }

    public async Task UpdatePasswordAsync(int userId, UpdatePasswordDto dto)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException($"Utilisateur introuvable (ID : {userId}).");

        if (!dto.CurrentPassword.VerifyHashProvided(user.PasswordHash))
            throw new UnauthorizedAccessException("Le mot de passe actuel est incorrect.");

        await _userRepository.UpdatePasswordAsync(userId, dto.NewPassword.GetHash());

        _logger.Info("Mot de passe mis à jour pour l'utilisateur ID {UserId}", userId);
    }

    // ── Admin ─────────────────────────────────────────────────────────────────

    public async Task<IEnumerable<AdminUserDto>> GetAllUsersExceptAsync(int currentAdminId)
    {
        var users = await _userRepository.GetAllExceptAsync(currentAdminId);
        return users.Select(ToAdminDto);
    }

    public async Task SetUserDisabledAsync(int targetUserId, bool isDisabled)
    {
        var exists = await _userRepository.GetByIdAsync(targetUserId)
            ?? throw new KeyNotFoundException($"Utilisateur introuvable (ID : {targetUserId}).");

        await _userRepository.SetDisabledAsync(targetUserId, isDisabled);
        _logger.Info("Utilisateur ID {UserId} — IsDisabled={IsDisabled}", targetUserId, isDisabled);
    }

    public async Task SetUserRoleAsync(int targetUserId, UserRole role)
    {
        var exists = await _userRepository.GetByIdAsync(targetUserId)
            ?? throw new KeyNotFoundException($"Utilisateur introuvable (ID : {targetUserId}).");

        await _userRepository.SetRoleAsync(targetUserId, role);
        _logger.Info("Utilisateur ID {UserId} — Role={Role}", targetUserId, role);
    }

    // ── Mapping helpers ───────────────────────────────────────────────────────

    private static UserProfileDto ToProfileDto(Domain.Entities.User u) => new()
    {
        Id               = u.Id,
        Email            = u.Email,
        FirstName        = u.FirstName,
        LastName         = u.LastName,
        Role             = u.Role.ToString(),
        IsEmailVerified  = u.IsEmailVerified,
        IsDisabled       = u.IsDisabled,
        TwoFactorEnabled = u.TwoFactorEnabled,
        CreatedAt        = u.CreatedAt,
    };

    private static AdminUserDto ToAdminDto(Domain.Entities.User u) => new()
    {
        Id              = u.Id,
        Email           = u.Email,
        FirstName       = u.FirstName,
        LastName        = u.LastName,
        Role            = u.Role.GetEnumDescription(),
        IsEmailVerified = u.IsEmailVerified,
        IsDisabled      = u.IsDisabled,
        // HasTwoFactor reflects CONFIRMED 2FA only, not a pending/unconfirmed secret.
        HasTwoFactor    = u.TwoFactorEnabled,
        CreatedAt       = u.CreatedAt,
    };
}