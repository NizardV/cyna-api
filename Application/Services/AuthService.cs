using Application.Dtos;
using Application.Interfaces;

using Domain.Entities;

using Infrastructure.Data; // À ajuster selon ton namespace AppDbContext
using Infrastructure.Entities;
using Infrastructure.Interfaces;
using Infrastructure.Security; // À ajuster selon l'emplacement de ton Hasher/JwtGenerator

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Tools;

namespace Application.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;

    private readonly ITokenGenerator _jwtTokenGenerator;

    public AuthService(
        AppDbContext context,
        ITokenGenerator jwtTokenGenerator)
    {
        _context = context;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<LoginResultDto> LoginAsync(LoginRequestDto request)
    {
        // 1. Chercher l'utilisateur
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower()); // Ou request.Username selon ton choix précédent

        if (user == null)
        {
            return new LoginResultDto { Success = false, ErrorMessage = "Identifiants invalides." };
        }

        // 2. Vérifier le mot de passe
        var isPasswordValid = request.Password.VerifyHashProvided(user.PasswordHash);
        if (!isPasswordValid)
        {
            return new LoginResultDto { Success = false, ErrorMessage = "Identifiants invalides." };
        }

        // 3. Générer le Token
        var token = _jwtTokenGenerator.GenerateToken(user);

        return new LoginResultDto
        {
            Success = true,
            Token = token
        };
    }

    public async Task<bool> RegisterAsync(RegisterRequestDto request)
    {
        // 1. Vérifier si l'utilisateur existe déjà
        var userExists = await _context.Users
            .AnyAsync(u => u.Email.ToLower() == request.Email.ToLower());

        if (userExists) return false;

        // 2. Séparer le FullName en FirstName et LastName pour coller à ton entité
        var parts = request.FullName.Trim().Split(' ', 2);
        var firstName = parts[0];
        var lastName = parts.Length > 1 ? parts[1] : string.Empty;

        // 3. Hasher le mot de passe
        // Note: Ajuste selon la méthode de ton outil (ex: HashPassword)
        var hashedPassword = request.Password.GetHash();

        // 4. Créer l'entité User
        var newUser = new User { 
            Email = request.Email,
            PasswordHash = hashedPassword,
            FirstName = firstName,
            LastName = lastName,
            Role = UserRole.User, // Valeur par défaut
            IsEmailVerified = false,
            CreatedAt = DateTime.UtcNow
        };

    _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<LoginResultDto?> ResetTokenAsync(ResetTokenRequestDto request)
    {
        // Pour le MVP axé Front-Office, on simule le rafraîchissement/génération rapide d'un token valide
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

        if (user == null) return null;

        var token = _jwtTokenGenerator.GenerateToken(user);

        return new LoginResultDto
        {
            Success = true,
            Token = token
        };
    }
}