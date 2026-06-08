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
/// Expose les routes de profil, de sécurité, de commandes et d'abonnements.
/// </summary>
[ApiController]
[Route("user")]
[Authorize]
[Produces("application/json")]
public class UserController : ControllerBase
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    private readonly IUserService _userService;
    private readonly IOrderService _orderService;
    private readonly ISubscriptionService _subscriptionService;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="UserController"/>.
    /// </summary>
    /// <param name="userService">Le service utilisateur.</param>
    /// <param name="orderService">Le service des commandes.</param>
    /// <param name="subscriptionService">Le service des abonnements.</param>
    public UserController(
        IUserService userService,
        IOrderService orderService,
        ISubscriptionService subscriptionService)
    {
        _userService           = userService;
        _orderService          = orderService;
        _subscriptionService   = subscriptionService;
    }

    // -------------------------------------------------------------------------
    // GET /user/profile
    // -------------------------------------------------------------------------

    /// <summary>
    /// Récupère le profil de l'utilisateur connecté.
    /// </summary>
    /// <returns>Le profil de l'utilisateur.</returns>
    /// <response code="200">Profil récupéré avec succès.</response>
    /// <response code="401">Utilisateur non authentifié.</response>
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
            _logger.Info("GET /user/profile — utilisateur ID {UserId}", userId);

            var profile = await _userService.GetProfileAsync(userId);
            return Ok(profile);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.Warn(ex, "Profil introuvable");
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.Warn(ex, "Accès non autorisé sur GET /user/profile");
            return Unauthorized(new { message = ex.Message });
        }
    }

    // -------------------------------------------------------------------------
    // PUT /user/profile
    // -------------------------------------------------------------------------

    /// <summary>
    /// Met à jour les informations personnelles de l'utilisateur connecté.
    /// </summary>
    /// <param name="dto">Les nouvelles valeurs du profil (prénom, nom, email).</param>
    /// <returns>Le profil mis à jour.</returns>
    /// <response code="200">Profil mis à jour avec succès.</response>
    /// <response code="400">Données invalides.</response>
    /// <response code="401">Utilisateur non authentifié.</response>
    /// <response code="404">Utilisateur introuvable.</response>
    [HttpPut("profile")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var userId = ClaimsHelper.GetUserId(User);
            _logger.Info("PUT /user/profile — utilisateur ID {UserId}", userId);

            var profile = await _userService.UpdateProfileAsync(userId, dto);
            return Ok(profile);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.Warn(ex, "Profil introuvable lors de la mise à jour");
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.Warn(ex, "Accès non autorisé sur PUT /user/profile");
            return Unauthorized(new { message = ex.Message });
        }
    }

    // -------------------------------------------------------------------------
    // PUT /user/password
    // -------------------------------------------------------------------------

    /// <summary>
    /// Met à jour le mot de passe de l'utilisateur connecté.
    /// Le mot de passe actuel doit être fourni pour validation.
    /// </summary>
    /// <param name="dto">Le mot de passe actuel et le nouveau mot de passe.</param>
    /// <returns>Un message de confirmation.</returns>
    /// <response code="200">Mot de passe mis à jour avec succès.</response>
    /// <response code="400">Données invalides ou mot de passe actuel incorrect.</response>
    /// <response code="401">Utilisateur non authentifié.</response>
    /// <response code="404">Utilisateur introuvable.</response>
    [HttpPut("password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var userId = ClaimsHelper.GetUserId(User);
            _logger.Info("PUT /user/password — utilisateur ID {UserId}", userId);

            await _userService.UpdatePasswordAsync(userId, dto);
            return Ok(new { message = "Mot de passe mis à jour avec succès." });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.Warn(ex, "Mot de passe actuel incorrect pour l'utilisateur");
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.Warn(ex, "Utilisateur introuvable lors du changement de mot de passe");
            return NotFound(new { message = ex.Message });
        }
    }

    // -------------------------------------------------------------------------
    // GET /user/orders
    // -------------------------------------------------------------------------

    /// <summary>
    /// Récupère l'historique des commandes de l'utilisateur connecté.
    /// </summary>
    /// <returns>La liste des commandes avec leurs articles et factures.</returns>
    /// <response code="200">Commandes récupérées avec succès.</response>
    /// <response code="401">Utilisateur non authentifié.</response>
    [HttpGet("orders")]
    [ProducesResponseType(typeof(IEnumerable<OrderSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetOrders()
    {
        try
        {
            var userId = ClaimsHelper.GetUserId(User);
            _logger.Info("GET /user/orders — utilisateur ID {UserId}", userId);

            var orders = await _orderService.GetUserOrdersAsync(userId);
            return Ok(orders);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.Warn(ex, "Accès non autorisé sur GET /user/orders");
            return Unauthorized(new { message = ex.Message });
        }
    }

    // -------------------------------------------------------------------------
    // GET /user/subscriptions
    // -------------------------------------------------------------------------

    /// <summary>
    /// Récupère les abonnements actifs de l'utilisateur connecté.
    /// </summary>
    /// <returns>La liste des abonnements.</returns>
    /// <response code="200">Abonnements récupérés avec succès.</response>
    /// <response code="401">Utilisateur non authentifié.</response>
    [HttpGet("subscriptions")]
    [ProducesResponseType(typeof(IEnumerable<SubscriptionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSubscriptions()
    {
        try
        {
            var userId = ClaimsHelper.GetUserId(User);
            _logger.Info("GET /user/subscriptions — utilisateur ID {UserId}", userId);

            var subscriptions = await _subscriptionService.GetUserSubscriptionsAsync(userId);
            return Ok(subscriptions);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.Warn(ex, "Accès non autorisé sur GET /user/subscriptions");
            return Unauthorized(new { message = ex.Message });
        }
    }
}