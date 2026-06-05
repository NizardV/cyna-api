using Application.Interfaces.Services;

using NLog;

namespace Application.Services;

using Domain.Dto.User;

using Infrastructure.Interfaces;

/// <summary>
/// Service de gestion de l'historique des commandes utilisateur.
/// Mappe les entités de la base de données vers les DTOs de l'API.
/// </summary>
public class OrderService : IOrderService
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    private readonly IOrderRepository _orderRepository;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="OrderService"/>.
    /// </summary>
    /// <param name="orderRepository">Le dépôt des commandes.</param>
    public OrderService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<OrderSummaryDto>> GetUserOrdersAsync(int userId)
    {
        _logger.Info("Récupération de l'historique des commandes pour l'utilisateur ID {UserId}", userId);

        var orders = await _orderRepository.GetByUserIdAsync(userId);

        return orders.Select(o => new OrderSummaryDto
        {
            Id          = o.Id,
            Status      = o.Status.ToString(),
            TotalAmount = o.TotalAmount,
            CreatedAt   = o.CreatedAt,
            InvoiceUrl  = o.Invoices.FirstOrDefault()?.PdfUrl,
            Items       = o.Items.Select(i => new OrderItemDto
            {
                Id                  = i.Id,
                ProductNameSnapshot = i.ProductNameSnapshot,
                PlanNameSnapshot    = i.PlanNameSnapshot,
                QuantityUsers       = i.QuantityUsers,
                QuantityDevices     = i.QuantityDevices,
            }).ToList(),
        });
    }
}