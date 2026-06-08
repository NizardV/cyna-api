using Infrastructure.Data;

using Microsoft.EntityFrameworkCore;
using NLog;

namespace Infrastructure.Repositories;

using Domain.Entities.AddressAndPayment;
using Domain.Entities.OrdersAndSubscriptions;

using Interfaces;

/// <summary>
/// Implémentation du dépôt des commandes via Entity Framework Core.
/// </summary>
public class OrderRepository : IOrderRepository
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    private readonly AppDbContext _context;

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

    /// <inheritdoc />
    public async Task<Order> SaveNewOrderAsync(
        Address billingAddress,
        Order order,
        IEnumerable<Subscription> subscriptions,
        int userId)
    {
        _logger.Debug("Création commande pour l'utilisateur ID {UserId}", userId);

        // 1. Adresse de facturation
        _context.Addresses.Add(billingAddress);
        await _context.SaveChangesAsync();

        // 2. Commande + articles
        order.BillingAddressId = billingAddress.Id;
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // 3. Abonnements
        var subList = subscriptions.ToList();
        if (subList.Count > 0)
        {
            _context.Subscriptions.AddRange(subList);
            await _context.SaveChangesAsync();
        }

        // 4. Vider le panier
        await _context.CartItems
            .Where(ci => ci.UserId == userId)
            .ExecuteDeleteAsync();

        _logger.Info("Commande ID {OrderId} créée avec succès", order.Id);
        return order;
    }
}