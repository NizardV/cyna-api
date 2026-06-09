namespace Application.Services;

using System.Security.Cryptography;

using Application.Interfaces;

using Domain.Dto.User;
using Domain.Entities;

using Infrastructure.Entities;
using Infrastructure.Interfaces;

using Tools;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenGenerator _jwtTokenGenerator;

    public AuthService(
        IUserRepository userRepository,
        ITokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResultDto> LoginAsync(LoginRequestDto request)
    {
        // 1. Chercher l'utilisateur via le Repository
        var user = await _userRepository.GetByEmailAsync(request.Email);

        if (user == null)
        {
            return new AuthResultDto { Success = false, ErrorMessage = "Identifiants invalides." };
        }

        // 2. Vérifier le mot de passe
        var isPasswordValid = request.Password.VerifyHashProvided(user.PasswordHash);
        if (!isPasswordValid)
        {
            return new AuthResultDto { Success = false, ErrorMessage = "Identifiants invalides." };
        }

        // 3. Générer le Token
        var token = _jwtTokenGenerator.GenerateToken(user);

        // 4. Générer un RefreshToken
        string refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        // 5. Sauvegarder le RefreshToken via le Repository
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1); // Valable 1 jour

        await _userRepository.UpdateAsync(user);


        return new AuthResultDto
        {
            Success = true,
            Token = token,
            RefreshToken = refreshToken
        };
    }

    public async Task<AuthResultDto> RegisterAsync(RegisterRequestDto request)
    {
        // 1. Vérifier si l'utilisateur existe déjà via le Repository
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user != null) return new AuthResultDto { Success = false, ErrorMessage = "Email déjà utilisé." };

        // 2. Séparer le FullName en FirstName et LastName
        var parts = request.FullName.Trim().Split(' ', 2);
        var firstName = parts[0];
        var lastName = parts.Length > 1 ? parts[1] : string.Empty;

        // 3. Hasher le mot de passe
        var hashedPassword = request.Password.GetHash();

        // 4. Créer l'entité User
        var newUser = new User
        {
            Email = request.Email,
            PasswordHash = hashedPassword,
            FirstName = firstName,
            LastName = lastName,
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

    public async Task<AuthResultDto?> ResetTokenAsync(RefreshTokenRequestDto request)
    {
        // Récupération via le Repository
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