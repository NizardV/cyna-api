using Application.Interfaces;
using Application.Interfaces.Services;

using Microsoft.AspNetCore.Mvc;

using NLog;

namespace Api.Controllers;

using Domain.Dto.Orders;

using Microsoft.AspNetCore.Authorization;

using Tools;

using ILogger = NLog.ILogger;

[ApiController]
[Route("orders")]
[Produces("application/json")]
public class OrderController : ControllerBase
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    private readonly IOrderService       _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    // -------------------------------------------------------------------------
    // POST /orders
    // -------------------------------------------------------------------------
    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(CreateOrderResultDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
