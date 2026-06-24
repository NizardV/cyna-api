namespace Infrastructure.Repositories;

using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using NLog;

using Domain.Entities;
using Domain.Entities.AuthCodes;

using Infrastructure.Interfaces;

using Tools;

/// <summary>
/// Implémentation du dépôt utilisateur via Entity Framework Core.
/// </summary>
public class UserRepository : IUserRepository
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    // ── Core user queries ─────────────────────────────────────────────────────

    public async Task<User?> GetByIdAsync(int userId)
    {
        _logger.Debug("GetByIdAsync — ID {UserId}", userId);
        return await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        _logger.Debug("GetByEmailAsync — {Email}", email);
        return await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByRefreshTokenAsync(string refreshToken)
    {
        return await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
    }

    public async Task<IEnumerable<User>> GetAllExceptAsync(int excludeId)
    {
        _logger.Debug("GetAllExceptAsync — excludeId={ExcludeId}", excludeId);
        return await _context.Users
            .AsNoTracking()
            .Where(u => u.Id != excludeId)
            .OrderBy(u => u.LastName).ThenBy(u => u.FirstName)
            .ToListAsync();
    }

    // ── Mutations ─────────────────────────────────────────────────────────────

    public async Task AddAsync(User user)
    {
        _logger.Debug("AddAsync — {Email}", user.Email);
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(User user)
    {
        _logger.Debug("UpdateAsync — ID {UserId}", user.Id);
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdatePasswordAsync(int userId, string newPasswordHash)
    {
        _logger.Debug("UpdatePasswordAsync — ID {UserId}", userId);
        await _context.Users
            .Where(u => u.Id == userId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.PasswordHash, newPasswordHash));
    }

    public async Task SetDisabledAsync(int userId, bool isDisabled)
    {
        _logger.Debug("SetDisabledAsync — ID {UserId}, IsDisabled={IsDisabled}", userId, isDisabled);
        await _context.Users
            .Where(u => u.Id == userId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsDisabled, isDisabled));
    }

    public async Task SetRoleAsync(int userId, UserRole role)
    {
        _logger.Debug("SetRoleAsync — ID {UserId}, Role={Role}", userId, role);
        await _context.Users
            .Where(u => u.Id == userId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.Role, role));
    }

    // ── Email verification codes ──────────────────────────────────────────────

    public async Task AddEmailVerificationCodeAsync(EmailVerificationCode code)
    {
        await _context.EmailVerificationCodes.AddAsync(code);
        await _context.SaveChangesAsync();
    }

    public async Task<EmailVerificationCode?> GetValidEmailVerificationCodeAsync(int userId, string code)
    {
        return await _context.EmailVerificationCodes
            .AsNoTracking()
            .Where(c => c.UserId    == userId
                     && c.Code      == code
                     && !c.IsUsed
                     && c.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(c => c.ExpiresAt)
            .FirstOrDefaultAsync();
    }

    public async Task MarkEmailVerificationCodeUsedAsync(int codeId)
    {
        await _context.EmailVerificationCodes
            .Where(c => c.Id == codeId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsUsed, true));
    }

    // ── Password reset codes ──────────────────────────────────────────────────

    public async Task AddPasswordResetCodeAsync(PasswordResetCode code)
    {
        await _context.PasswordResetCodes.AddAsync(code);
        await _context.SaveChangesAsync();
    }

    public async Task<PasswordResetCode?> GetValidPasswordResetCodeAsync(string email, string code)
    {
        return await _context.PasswordResetCodes
            .AsNoTracking()
            .Include(c => c.User)
            .Where(c => c.User.Email == email
                     && c.Code       == code
                     && !c.IsUsed
                     && c.ExpiresAt  > DateTime.UtcNow)
            .OrderByDescending(c => c.ExpiresAt)
            .FirstOrDefaultAsync();
    }

    public async Task MarkPasswordResetCodeUsedAsync(int codeId)
    {
        await _context.PasswordResetCodes
            .Where(c => c.Id == codeId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsUsed, true));
    }
}