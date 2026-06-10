using Application.Interfaces;
using Application.Interfaces.Services;

using Microsoft.AspNetCore.Mvc;

using NLog;

namespace Api.Controllers;

using Domain.Dto.Cart;

using Microsoft.AspNetCore.Authorization;

using Tools;

using ILogger = NLog.ILogger;

[ApiController]
[Route("cart")]
[Produces("application/json")]
public class CartController : ControllerBase
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    private readonly ICartService        _cartService;
    private readonly ICurrentUserService _currentUser;

    public CartController(ICartService cartService, ICurrentUserService currentUser)
    {
        _cartService = cartService;
        _currentUser = currentUser;
    }

    // -------------------------------------------------------------------------
    // POST /cart
    // -------------------------------------------------------------------------
    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(CartResultDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddToCart([FromBody] AddCartItemRequestDto dto)
    {
        try
        {
            var userId = ClaimsHelper.GetUserId(User);
            _logger.Info("POST /cart — userId={UserId}, planId={PlanId}", userId, dto.PricingPlanId);

            var result = await _cartService.AddOrUpdateCartItemAsync(userId, dto);
            return StatusCode(StatusCodes.Status201Created, result);
        }
        catch (ArgumentException ex)
        {
            _logger.Warn(ex, "Quantités invalides sur POST /cart");
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.Warn(ex, "Plan introuvable sur POST /cart");
            return NotFound(new { message = ex.Message });
        }
    }
}
