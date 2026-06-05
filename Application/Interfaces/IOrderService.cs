namespace Application.Interfaces.Services;

using Domain.Dto.Orders;
using Domain.Dto.User;

/// <summary>
/// Interface du service de gestion des commandes utilisateur.
/// </summary>
public interface IOrderService
{
    /// <summary>Récupère l'historique des commandes de l'utilisateur.</summary>
    Task<IEnumerable<OrderSummaryDto>> GetUserOrdersAsync(int userId);

    /// <summary>Crée une commande depuis le panier validé.</summary>
    Task<CreateOrderResultDto> CreateOrderAsync(int userId, CreateOrderRequestDto dto);
}