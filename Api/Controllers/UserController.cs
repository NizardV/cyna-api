using Tools;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NLog;

namespace Api.Controllers;

using Application.Interfaces;

using Domain.Dto.User;

using ILogger = NLog.ILogger;

/// <summary>
/// Contrôleur de gestion du compte utilisateur.
/// Expose les routes de profil, sécurité, commandes et abonnements.
/// </summary>
[ApiController]
[Route("user")]
[Authorize]
[Produces("application/json")]
public class UserController : ControllerBase
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    private readonly IUserService         _userService;
    private readonly IOrderService        _orderService;
    private readonly ISubscriptionService _subscriptionService;

    public UserController(
        IUserService         userService,
        IOrderService        orderService,
        ISubscriptionService subscriptionService)
    {
        _userService         = userService;
        _orderService        = orderService;
        _subscriptionService = subscriptionService;
    }

    // ── GET /user/profile ─────────────────────────────────────────────────────

    /// <summary>Récupère le profil de l'utilisateur connecté.</summary>
    /// <response code="200">Profil récupéré.</response>
    /// <response code="401">Non authentifié.</response>
    /// <response code="404">Utilisateur introuvable.</response>
    [HttpGet("profile")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfile()
    {
        try
        {
            var userId = ClaimsHelper.GetUserId(User);
            _logger.Info("GET /user/profile — ID {UserId}", userId);
            return Ok(await _userService.GetProfileAsync(userId));
        }
        catch (KeyNotFoundException ex)       { return NotFound(new { error = ex.Message }); }
        catch (UnauthorizedAccessException ex) { return Unauthorized(new { error = ex.Message }); }
    }

    // ── PUT /user/profile ─────────────────────────────────────────────────────

    /// <summary>
    /// Met à jour le profil de l'utilisateur connecté.
    /// Si l'adresse email change, IsEmailVerified passe à false et un nouveau code OTP est envoyé.
    /// </summary>
    /// <response code="200">Profil mis à jour.</response>
    /// <response code="400">Données invalides.</response>
    /// <response code="401">Non authentifié.</response>
    /// <response code="404">Utilisateur introuvable.</response>
    [HttpPut("profile")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var userId = ClaimsHelper.GetUserId(User);
            _logger.Info("PUT /user/profile — ID {UserId}", userId);
            return Ok(await _userService.UpdateProfileAsync(userId, dto));
        }
        catch (KeyNotFoundException ex)       { return NotFound(new { error = ex.Message }); }
        catch (UnauthorizedAccessException ex) { return Unauthorized(new { error = ex.Message }); }
    }

    // ── PUT /user/password ────────────────────────────────────────────────────

    /// <summary>
    /// Met à jour le mot de passe de l'utilisateur connecté (requiert l'ancien mot de passe).
    /// </summary>
    /// <response code="200">Mot de passe mis à jour.</response>
    /// <response code="400">Ancien mot de passe incorrect ou données invalides.</response>
    /// <response code="401">Non authentifié.</response>
    /// <response code="404">Utilisateur introuvable.</response>
    [HttpPut("password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var userId = ClaimsHelper.GetUserId(User);
            _logger.Info("PUT /user/password — ID {UserId}", userId);
            await _userService.UpdatePasswordAsync(userId, dto);
            return Ok(new { message = "Mot de passe mis à jour avec succès." });
        }
        catch (UnauthorizedAccessException ex) { return BadRequest(new { error = ex.Message }); }
        catch (KeyNotFoundException ex)         { return NotFound(new { error = ex.Message }); }
    }

    // ── GET /user/orders ──────────────────────────────────────────────────────

    /// <summary>Retourne l'historique des commandes de l'utilisateur connecté.</summary>
    /// <response code="200">Commandes récupérées.</response>
    /// <response code="401">Non authentifié.</response>
    [HttpGet("orders")]
    [ProducesResponseType(typeof(IEnumerable<OrderSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetOrders()
    {
        try
        {
            var userId = ClaimsHelper.GetUserId(User);
            _logger.Info("GET /user/orders — ID {UserId}", userId);
            return Ok(await _orderService.GetUserOrdersAsync(userId));
        }
        catch (UnauthorizedAccessException ex) { return Unauthorized(new { error = ex.Message }); }
    }

    // ── GET /user/subscriptions ───────────────────────────────────────────────

    /// <summary>Retourne les abonnements actifs de l'utilisateur connecté.</summary>
    /// <response code="200">Abonnements récupérés.</response>
    /// <response code="401">Non authentifié.</response>
    [HttpGet("subscriptions")]
    [ProducesResponseType(typeof(IEnumerable<SubscriptionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSubscriptions()
    {
        try
        {
            var userId = ClaimsHelper.GetUserId(User);
            _logger.Info("GET /user/subscriptions — ID {UserId}", userId);
            return Ok(await _subscriptionService.GetUserSubscriptionsAsync(userId));
        }
        catch (UnauthorizedAccessException ex) { return Unauthorized(new { error = ex.Message }); }
    }
}