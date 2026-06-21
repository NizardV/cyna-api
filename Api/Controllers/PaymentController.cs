using Application.Interfaces.Services;

using Infrastructure.Payments;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using NLog;

using Stripe;

namespace Api.Controllers;

using Domain.Dto.Payments;

using Tools;

using ILogger = NLog.ILogger;

/// <summary>
/// Contrôleur de paiement.
/// Initialise le paiement par abonnement à partir du panier de l'utilisateur connecté.
/// </summary>
[ApiController]
[Route("payments")]
[Produces("application/json")]
public class PaymentController : ControllerBase
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    private readonly ICheckoutService _checkoutService;
    private readonly StripeOptions _stripeOptions;

    public PaymentController(ICheckoutService checkoutService, IOptions<StripeOptions> stripeOptions)
    {
        _checkoutService = checkoutService;
        _stripeOptions   = stripeOptions.Value;
    }

    /// <summary>
    /// Initialise le paiement par abonnement : crée le(s) abonnement(s) Stripe et renvoie
    /// le(s) client secret(s) à confirmer côté front, ainsi que la clé publiable.
    /// </summary>
    /// <response code="201">Paiement initialisé : client secret(s) renvoyé(s).</response>
    /// <response code="400">Panier vide ou sans ligne facturable.</response>
    /// <response code="401">Utilisateur non authentifié.</response>
    /// <response code="404">Utilisateur ou plan tarifaire introuvable.</response>
    /// <response code="502">Erreur du fournisseur de paiement.</response>
    [Authorize]
    [HttpPost("subscription")]
    [ProducesResponseType(typeof(CheckoutPaymentResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateSubscriptionPayment()
    {
        try
        {
            var userId = ClaimsHelper.GetUserId(User);
            _logger.Info("POST /payments/subscription — userId={UserId}", userId);

            var result = await _checkoutService.InitSubscriptionPaymentAsync(userId);

            return StatusCode(StatusCodes.Status201Created, new CheckoutPaymentResponseDto
            {
                ClientSecret    = result.ClientSecrets.FirstOrDefault(),
                ClientSecrets   = result.ClientSecrets,
                SubscriptionIds = result.SubscriptionIds,
                PublishableKey  = _stripeOptions.PublishableKey,
            });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.Warn(ex, "Ressource introuvable sur POST /payments/subscription");
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.Warn(ex, "État invalide sur POST /payments/subscription");
            return BadRequest(new { message = ex.Message });
        }
        catch (StripeException ex)
        {
            _logger.Error(ex, "Erreur Stripe sur POST /payments/subscription");
            return StatusCode(StatusCodes.Status502BadGateway, new { message = "Erreur du fournisseur de paiement." });
        }
    }
}
