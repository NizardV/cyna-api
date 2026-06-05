using Domain.Repositories;
using Infrastructure.Data;
using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace Infrastructure.Repositories;

using Domain.Entities;

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
    public async Task UpdateProfileAsync(User user)
    {
        _logger.Debug("Mise à jour du profil de l'utilisateur ID {UserId}", user.Id);

        _context.Users.Update(user);
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
}