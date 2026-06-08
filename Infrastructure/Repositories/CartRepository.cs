using Infrastructure.Data;
using Infrastructure.Interfaces;

using Microsoft.EntityFrameworkCore;

using NLog;

namespace Infrastructure.Repositories;

using Domain.Entities.Catalogue;
using Domain.Entities.OrdersAndSubscriptions;

public class CartRepository : ICartRepository
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    private readonly AppDbContext _context;

    public CartRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<PricingPlan?> GetPricingPlanWithTiersAsync(int pricingPlanId)
    {
        _logger.Debug("Chargement du plan tarifaire ID {PlanId}", pricingPlanId);

        return await _context.PricingPlans
            .AsNoTracking()
            .Include(pp => pp.PricingTiers)
            .Include(pp => pp.Product)
                .ThenInclude(p => p.Translations)
            .FirstOrDefaultAsync(pp => pp.Id == pricingPlanId);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<CartItem>> GetCartItemsAsync(int userId)
    {
        _logger.Debug("Chargement du panier pour l'utilisateur ID {UserId}", userId);

        return await _context.CartItems
            .AsNoTracking()
            .Where(ci => ci.UserId == userId)
            .Include(ci => ci.PricingPlan)
                .ThenInclude(pp => pp.PricingTiers)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<CartItem> UpsertCartItemAsync(
        int userId, int productId, int pricingPlanId,
        int quantityUsers, int quantityDevices)
    {
        var existing = await _context.CartItems
            .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.PricingPlanId == pricingPlanId);

        if (existing is not null)
        {
            _logger.Debug("Mise à jour CartItem existant ID {Id}", existing.Id);
            existing.QuantityUsers   = quantityUsers;
            existing.QuantityDevices = quantityDevices;
            await _context.SaveChangesAsync();
            return existing;
        }

        var item = new CartItem
        {
            UserId        = userId,
            ProductId     = productId,
            PricingPlanId = pricingPlanId,
            QuantityUsers = quantityUsers,
            QuantityDevices = quantityDevices,
        };

        _context.CartItems.Add(item);
        await _context.SaveChangesAsync();

        _logger.Debug("Nouveau CartItem créé ID {Id}", item.Id);
        return item;
    }

    /// <inheritdoc />
    public async Task ClearCartAsync(int userId)
    {
        _logger.Debug("Vidage du panier pour l'utilisateur ID {UserId}", userId);

        await _context.CartItems
            .Where(ci => ci.UserId == userId)
            .ExecuteDeleteAsync();
    }
}
