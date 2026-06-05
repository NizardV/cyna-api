using Infrastructure.Data;

using Microsoft.EntityFrameworkCore;
using NLog;

namespace Infrastructure.Repositories;

using Domain.Entities.OrdersAndSubscriptions;

using Interfaces;

/// <summary>
/// Implémentation du dépôt des abonnements via Entity Framework Core.
/// </summary>
public class SubscriptionRepository : ISubscriptionRepository
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    private readonly AppDbContext _context;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="SubscriptionRepository"/>.
    /// </summary>
    /// <param name="context">Le contexte de base de données.</param>
    public SubscriptionRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Subscription>> GetByUserIdAsync(int userId)
    {
        _logger.Debug("Récupération des abonnements pour l'utilisateur ID {UserId}", userId);

        return await _context.Subscriptions
            .AsNoTracking()
            .Where(s => s.UserId == userId)
            .Include(s => s.Product)
            .ThenInclude(p => p.Translations)
            .Include(s => s.PricingPlan)
            .OrderByDescending(s => s.CurrentPeriodEnd)
            .ToListAsync();
    }
}