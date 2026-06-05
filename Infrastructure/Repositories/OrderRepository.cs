using Infrastructure.Data;

using Microsoft.EntityFrameworkCore;
using NLog;

namespace Infrastructure.Repositories;

using Domain.Entities.OrdersAndSubscriptions;

using Interfaces;

/// <summary>
/// Implémentation du dépôt des commandes via Entity Framework Core.
/// </summary>
public class OrderRepository : IOrderRepository
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    private readonly AppDbContext _context;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="OrderRepository"/>.
    /// </summary>
    /// <param name="context">Le contexte de base de données.</param>
    public OrderRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Order>> GetByUserIdAsync(int userId)
    {
        _logger.Debug("Récupération des commandes pour l'utilisateur ID {UserId}", userId);

        return await _context.Orders
            .AsNoTracking()
            .Where(o => o.UserId == userId)
            .Include(o => o.Items)
            .ThenInclude(i => i.PricingPlan)
            .Include(o => o.Invoices)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<Order?> GetByIdAsync(int orderId, int userId)
    {
        _logger.Debug("Récupération de la commande ID {OrderId} pour l'utilisateur ID {UserId}", orderId, userId);

        return await _context.Orders
            .AsNoTracking()
            .Where(o => o.Id == orderId && o.UserId == userId)
            .Include(o => o.Items)
            .ThenInclude(i => i.PricingPlan)
            .Include(o => o.Invoices)
            .Include(o => o.PromoCodes)
            .ThenInclude(p => p.PromoCode)
            .FirstOrDefaultAsync();
    }
}