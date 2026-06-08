namespace Infrastructure.Interfaces;

using Domain.Entities.Catalogue;
using Domain.Entities.OrdersAndSubscriptions;

public interface ICartRepository
{
    Task<PricingPlan?> GetPricingPlanWithTiersAsync(int pricingPlanId);
    Task<IEnumerable<CartItem>> GetCartItemsAsync(int userId);
    Task<CartItem> UpsertCartItemAsync(int userId, int productId, int pricingPlanId, int quantityUsers, int quantityDevices);
    Task ClearCartAsync(int userId);
}
