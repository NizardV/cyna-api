using Application.Interfaces;
using Application.Interfaces.Services;

using Microsoft.AspNetCore.Mvc;

using NLog;

namespace Api.Controllers;

using Domain.Dto.Orders;

using Microsoft.AspNetCore.Authorization;

using Tools;

using ILogger = NLog.ILogger;

/// <summary>
/// Contrôleur de gestion des commandes.
/// Permet à un utilisateur authentifié de transformer son panier en commande.
/// </summary>
[ApiController]
[Route("orders")]
[Produces("application/json")]
public class OrderController : ControllerBase
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    private readonly IOrderService _orderService;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="OrderController"/>.
    /// </summary>
    /// <param name="orderService">Le service de gestion des commandes.</param>
    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Crée une commande à partir du panier validé de l'utilisateur connecté.
    /// Valide les quantités, calcule les totaux TTC et crée les abonnements pour les plans récurrents.
    /// </summary>
    /// <param name="dto">Les articles du panier, l'adresse de facturation et l'identifiant de paiement Stripe.</param>
    /// <returns>Le résumé de la commande créée.</returns>
    /// <response code="201">Commande créée avec succès.</response>
    /// <response code="400">Quantités invalides ou dépassement du seuil de commande directe.</response>
    /// <response code="401">Utilisateur non authentifié.</response>
    /// <response code="404">Plan tarifaire introuvable.</response>
    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(CreateOrderResultDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequestDto dto)
    {
        try
        {
            var userId = ClaimsHelper.GetUserId(User);
            _logger.Info("POST /orders — userId={UserId}", userId);

            var result = await _orderService.CreateOrderAsync(userId, dto);
            return StatusCode(StatusCodes.Status201Created, result);
        }
        catch (ArgumentException ex)
        {
            _logger.Warn(ex, "Quantités invalides sur POST /orders");
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.Warn(ex, "Dépassement seuil sur POST /orders");
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.Warn(ex, "Plan introuvable sur POST /orders");
            return NotFound(new { message = ex.Message });
        }
    }
}
