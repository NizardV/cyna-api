using Application.Interfaces;
using Application.Interfaces.Services;

using Microsoft.AspNetCore.Mvc;

using NLog;

namespace Api.Controllers;

using Domain.Dto.Cart;

using Microsoft.AspNetCore.Authorization;

using Tools;

using ILogger = NLog.ILogger;

/// <summary>
/// Contrôleur de gestion du panier d'achat.
/// Permet à un utilisateur authentifié d'ajouter ou de mettre à jour un article dans son panier.
/// </summary>
[ApiController]
[Route("cart")]
[Produces("application/json")]
public class CartController : ControllerBase
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    private readonly ICartService _cartService;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="CartController"/>.
    /// </summary>
    /// <param name="cartService">Le service de gestion du panier.</param>
    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    /// <summary>
    /// Ajoute un article au panier ou met à jour les quantités si le plan tarifaire est déjà présent.
    /// </summary>
    /// <param name="dto">Le plan tarifaire et les quantités (utilisateurs et/ou appareils).</param>
    /// <returns>L'état mis à jour du panier avec le récapitulatif des montants.</returns>
    /// <response code="201">Article ajouté ou mis à jour avec succès.</response>
    /// <response code="400">Quantités invalides (toutes à zéro).</response>
    /// <response code="401">Utilisateur non authentifié.</response>
    /// <response code="404">Plan tarifaire introuvable.</response>
    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(CartResultDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
