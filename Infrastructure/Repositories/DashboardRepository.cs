using Infrastructure.Data;
using Infrastructure.Entities;

using Microsoft.EntityFrameworkCore;

using NLog;

namespace Infrastructure.Repositories;

using Domain.Dto.Dashboard;

using Interfaces;

using Tools;

/// <summary>
/// Implémentation EF Core du dépôt de statistiques du dashboard admin.
/// Toutes les requêtes filtrent sur <c>CreatedAt</c> (commandes, abonnements) ou
/// l'équivalent pour les utilisateurs, entre les bornes [start, end).
/// </summary>
public class DashboardRepository : IDashboardRepository
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    private readonly AppDbContext _context;

    public DashboardRepository(AppDbContext context)
    {
        _context = context;
    }

    // -------------------------------------------------------------------------
    // CA
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public async Task<RevenueStatsDto> GetRevenueStatsAsync(
        DateTime start, DateTime end, DateTime previousStart, DateTime previousEnd)
    {
        _logger.Debug("Calcul CA — start={Start}, end={End}", start, end);

        var paidOrders = _context.Orders
            .AsNoTracking()
            .Where(o => o.Status == OrderStatus.Paid);

        var total = await paidOrders.SumAsync(o => (decimal?)o.TotalAmount) ?? 0m;

        var currentPeriod = await paidOrders
            .Where(o => o.CreatedAt >= start && o.CreatedAt < end)
            .SumAsync(o => (decimal?)o.TotalAmount) ?? 0m;

        var previousPeriod = await paidOrders
            .Where(o => o.CreatedAt >= previousStart && o.CreatedAt < previousEnd)
            .SumAsync(o => (decimal?)o.TotalAmount) ?? 0m;

        var growthPercent = previousPeriod == 0m
            ? (currentPeriod > 0m ? 100m : 0m)
            : Math.Round((currentPeriod - previousPeriod) / previousPeriod * 100m, 2);

        var byMonth = await paidOrders
            .Where(o => o.CreatedAt >= start && o.CreatedAt < end)
            .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
            .Select(g => new MonthlyRevenueDto
            {
                Year    = g.Key.Year,
                Month   = g.Key.Month,
                Revenue = g.Sum(o => o.TotalAmount),
            })
            .OrderBy(m => m.Year).ThenBy(m => m.Month)
            .ToListAsync();

        return new RevenueStatsDto
        {
            Total          = total,
            CurrentPeriod  = currentPeriod,
            PreviousPeriod = previousPeriod,
            GrowthPercent  = growthPercent,
            ByMonth        = byMonth,
        };
    }

    // -------------------------------------------------------------------------
    // Orders
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public async Task<OrderStatsDto> GetOrderStatsAsync(DateTime start, DateTime end)
    {
        _logger.Debug("Calcul stats commandes — start={Start}, end={End}", start, end);

        var ordersInPeriod = _context.Orders
            .AsNoTracking()
            .Where(o => o.CreatedAt >= start && o.CreatedAt < end);

        var total = await ordersInPeriod.CountAsync();

        var byStatusRaw = await ordersInPeriod
            .GroupBy(o => o.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        var byStatus = byStatusRaw.ToDictionary(
            x => x.Status.ToString().ToLowerInvariant(),
            x => x.Count);

        var byMonth = await ordersInPeriod
            .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
            .Select(g => new MonthlyOrderCountDto
            {
                Year  = g.Key.Year,
                Month = g.Key.Month,
                Count = g.Count(),
            })
            .OrderBy(m => m.Year).ThenBy(m => m.Month)
            .ToListAsync();

        return new OrderStatsDto
        {
            Total    = total,
            ByStatus = byStatus,
            ByMonth  = byMonth,
        };
    }

    // -------------------------------------------------------------------------
    // Users
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public async Task<UserStatsDto> GetUserStatsAsync(DateTime start, DateTime end)
    {
        _logger.Debug("Calcul stats utilisateurs — start={Start}, end={End}", start, end);

        var users = _context.Users.AsNoTracking();

        var total         = await users.CountAsync();
        var verifiedEmail = await users.CountAsync(u => u.IsEmailVerified);
        var newInPeriod   = await users.CountAsync(u => u.CreatedAt >= start && u.CreatedAt < end);

        var byMonth = await users
            .Where(u => u.CreatedAt >= start && u.CreatedAt < end)
            .GroupBy(u => new { u.CreatedAt.Year, u.CreatedAt.Month })
            .Select(g => new MonthlyUserCountDto
            {
                Year  = g.Key.Year,
                Month = g.Key.Month,
                Count = g.Count(),
            })
            .OrderBy(m => m.Year).ThenBy(m => m.Month)
            .ToListAsync();

        return new UserStatsDto
        {
            Total         = total,
            NewInPeriod   = newInPeriod,
            VerifiedEmail = verifiedEmail,
            ByMonth       = byMonth,
        };
    }

    // -------------------------------------------------------------------------
    // Subscriptions
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public async Task<SubscriptionStatsDto> GetSubscriptionStatsAsync(DateTime start, DateTime end)
    {
        _logger.Debug("Calcul stats abonnements — start={Start}, end={End}", start, end);

        var subscriptions = _context.Subscriptions.AsNoTracking();

        var subsInPeriod = subscriptions.Where(s => s.CurrentPeriodStart >= start && s.CurrentPeriodStart < end);

        var total  = await subsInPeriod.CountAsync();
        var active = await subscriptions.CountAsync(s => s.Status == SubscriptionStatus.Active);

        var byStatusRaw = await subsInPeriod
            .GroupBy(s => s.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        var byStatus = byStatusRaw.ToDictionary(
            x => x.Status.ToString().ToLowerInvariant(),
            x => x.Count);

        var byMonth = await subsInPeriod
            .GroupBy(s => new { s.CurrentPeriodStart.Year, s.CurrentPeriodStart.Month })
            .Select(g => new MonthlySubscriptionCountDto
            {
                Year  = g.Key.Year,
                Month = g.Key.Month,
                Count = g.Count(),
            })
            .OrderBy(m => m.Year).ThenBy(m => m.Month)
            .ToListAsync();

        return new SubscriptionStatsDto
        {
            Total    = total,
            Active   = active,
            ByStatus = byStatus,
            ByMonth  = byMonth,
        };
    }

    // -------------------------------------------------------------------------
    // Top products
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public async Task<IEnumerable<TopProductDto>> GetTopProductsAsync(
        DateTime start, DateTime end, TopProductSortBy sortBy, int limit)
    {
        _logger.Debug("Calcul top produits — start={Start}, end={End}, sortBy={SortBy}, limit={Limit}", start, end, sortBy, limit);

        // Lignes de commandes payées, dans la période, jointes à leur commande pour filtrer sur CreatedAt/Status.
        var lines = _context.OrderItems
            .AsNoTracking()
            .Where(oi => oi.Order.Status == OrderStatus.Paid
                      && oi.Order.CreatedAt >= start
                      && oi.Order.CreatedAt < end);

        var grouped = lines
            .GroupBy(oi => new { oi.ProductId, oi.ProductNameSnapshot })
            .Select(g => new
            {
                g.Key.ProductId,
                g.Key.ProductNameSnapshot,
                Revenue = g.Sum(oi => oi.UnitPriceUsers * oi.QuantityUsers + oi.UnitPriceDevices * oi.QuantityDevices),
                OrdersCount = g.Select(oi => oi.OrderId).Distinct().Count(),
            });

        grouped = sortBy == TopProductSortBy.Orders
            ? grouped.OrderByDescending(x => x.OrdersCount)
            : grouped.OrderByDescending(x => x.Revenue);

        var top = await grouped.Take(limit).ToListAsync();

        // Récupère les images en une requête séparée (évite un join compliqué côté SQL).
        var productIds = top.Select(x => x.ProductId).ToList();
        var images = await _context.Products
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .Select(p => new { p.Id, ImageUrl = p.Images.OrderBy(i => i.DisplayOrder).Select(i => i.ImageUrl).FirstOrDefault() })
            .ToDictionaryAsync(x => x.Id, x => x.ImageUrl);

        return top.Select(x => new TopProductDto
        {
            ProductId   = x.ProductId,
            ProductName = x.ProductNameSnapshot,
            ImageUrl    = images.GetValueOrDefault(x.ProductId),
            Revenue     = x.Revenue,
            OrdersCount = x.OrdersCount,
        });
    }
}