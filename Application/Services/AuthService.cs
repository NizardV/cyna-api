namespace Application.Services;

using System.Security.Cryptography;

using Application.Interfaces;

using Domain.Dto.User;
using Domain.Entities;

using Infrastructure.Entities;
using Infrastructure.Interfaces;

using Tools;

/// <summary>
/// Implémentation du service d'authentification.
/// Gère la connexion, l'inscription, le renouvellement de token et la déconnexion.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenGenerator _jwtTokenGenerator;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="AuthService"/>.
    /// </summary>
    /// <param name="userRepository">Le dépôt utilisateur.</param>
    /// <param name="jwtTokenGenerator">Le générateur de tokens JWT.</param>
    public AuthService(
        IUserRepository userRepository,
        ITokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    /// <inheritdoc />
    public async Task<AuthResultDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);

        if (user == null)
        {
            return new AuthResultDto { Success = false, ErrorMessage = "Identifiants invalides." };
        }

        var isPasswordValid = request.Password.VerifyHashProvided(user.PasswordHash);
        if (!isPasswordValid)
        {
            return new AuthResultDto { Success = false, ErrorMessage = "Identifiants invalides." };
        }

        var token = _jwtTokenGenerator.GenerateToken(user);
        string refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1);

        await _userRepository.UpdateAsync(user);

        return new AuthResultDto
        {
            Success = true,
            Token = token,
            RefreshToken = refreshToken
        };
    }

    /// <inheritdoc />
    public async Task<AuthResultDto> RegisterAsync(RegisterRequestDto request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user != null) return new AuthResultDto { Success = false, ErrorMessage = "Email déjà utilisé." };

        var hashedPassword = request.Password.GetHash();

        var newUser = new User
        {
            Email = request.Email,
            PasswordHash = hashedPassword,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Role = UserRole.User,
            IsEmailVerified = false,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(newUser);

        return new AuthResultDto
        {
            Success = true,
            Token = null,
            RefreshToken = null
        };
    }

    /// <inheritdoc />
    public async Task<AuthResultDto?> ResetTokenAsync(RefreshTokenRequestDto request)
    {
        var user = await _userRepository.GetByRefreshTokenAsync(request.RefreshToken);

        if (user == null || user.RefreshTokenExpiryTime < DateTime.UtcNow) return null;

        var token = _jwtTokenGenerator.GenerateToken(user);
        string refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1);

        await _userRepository.UpdateAsync(user);

        return new AuthResultDto
        {
            Success = true,
            Token = token,
            RefreshToken = refreshToken
        };
    }

    /// <inheritdoc />
    public async Task<bool> LogoutAsync(string refreshToken)
    {
        var user = await _userRepository.GetByRefreshTokenAsync(refreshToken);
        if (user == null) return false;

        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;

        await _userRepository.UpdateAsync(user);
        return true;
    }
}