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

    /// <inheritdoc />
    public async Task<Order> SaveNewOrderAsync(
        Address billingAddress,
        Order order,
        IEnumerable<Subscription> subscriptions,
        int userId)
    {
        _logger.Debug("Création commande pour l'utilisateur ID {UserId}", userId);

        _context.Addresses.Add(billingAddress);
        await _context.SaveChangesAsync();

        order.BillingAddressId = billingAddress.Id;
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        var subList = subscriptions.ToList();
        if (subList.Count > 0)
        {
            _context.Subscriptions.AddRange(subList);
            await _context.SaveChangesAsync();
        }

        await _context.CartItems
            .Where(ci => ci.UserId == userId)
            .ExecuteDeleteAsync();

        _logger.Info("Commande ID {OrderId} créée avec succès", order.Id);
        return order;
    }

    /// <inheritdoc />
    public async Task<Order> CreatePendingOrderAsync(Address billingAddress, Order order)
    {
        _context.Addresses.Add(billingAddress);
        await _context.SaveChangesAsync();

        order.BillingAddressId = billingAddress.Id;
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        _logger.Info("Commande Pending ID {OrderId} créée", order.Id);
        return order;
    }

    /// <inheritdoc />
    public async Task<Order?> GetTrackedByIdAsync(int orderId)
        => await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);

    /// <inheritdoc />
    public async Task<Order?> GetByStripePaymentIntentIdAsync(string paymentIntentId)
        => await _context.Orders.FirstOrDefaultAsync(o => o.StripePaymentIntentId == paymentIntentId);

    /// <inheritdoc />
    public async Task UpdateOrderAsync(Order order)
    {
        _context.Orders.Update(order);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<bool> InvoiceExistsForOrderAsync(int orderId)
        => await _context.Invoices.AnyAsync(i => i.OrderId == orderId);

    /// <inheritdoc />
    public async Task AddInvoiceAsync(Invoice invoice)
    {
        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();
    }
}