namespace Infrastructure.Repositories;

using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using NLog;

using Domain.Entities;

using Infrastructure.Interfaces;

/// <summary>
/// Implémentation du dépôt utilisateur via Entity Framework Core.
/// </summary>
public class UserRepository : IUserRepository
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    private readonly AppDbContext _context;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="UserRepository"/>.
    /// </summary>
    /// <param name="context">Le contexte de base de données.</param>
    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<User?> GetByIdAsync(int userId)
    {
        _logger.Debug("Récupération de l'utilisateur avec l'ID {UserId}", userId);

        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    /// <inheritdoc />
    public async Task<User?> GetByEmailAsync(string email)
    {
        _logger.Debug("Récupération de l'utilisateur avec l'email {Email}", email);
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    /// <inheritdoc />
    public async Task<User?> GetByRefreshTokenAsync(string refreshToken)
    {
        _logger.Debug("Récupération de l'utilisateur avec le refresh token {RefreshToken}", refreshToken);
        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(User user)
    {
        _logger.Debug("Ajout d'un nouvel utilisateur avec l'email {Email}", user.Email);
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task UpdatePasswordAsync(int userId, string newPasswordHash)
    {
        _logger.Debug("Mise à jour du mot de passe pour l'utilisateur ID {UserId}", userId);

        await _context.Users
            .Where(u => u.Id == userId)
            .ExecuteUpdateAsync(u => u.SetProperty(x => x.PasswordHash, newPasswordHash));
    }

    /// <inheritdoc />
    public async Task UpdateAsync(User user)
    {
        _logger.Debug("Mise à jour de l'utilisateur ID {UserId}", user.Id);
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }
}