using NLog;

namespace Application.Services;

using Domain.Dto.User;

using Infrastructure.Interfaces;

using Interfaces;

/// <summary>
/// Service de gestion de l'historique des commandes utilisateur.
/// Mappe les entités de la base de données vers les DTOs de l'API.
/// La création de commande est désormais pilotée par le paiement (CheckoutService + webhook).
/// </summary>
public class OrderService : IOrderService
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    private readonly IOrderRepository _orderRepository;

    public OrderService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<OrderSummaryDto>> GetUserOrdersAsync(int userId)
    {
        _logger.Info("Récupération des commandes pour l'utilisateur ID {UserId}", userId);

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
