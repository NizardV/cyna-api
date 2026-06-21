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
/// Contrôleur de paiement : initialisation du paiement par abonnement et réception des webhooks Stripe.
/// </summary>
[ApiController]
[Route("payments")]
[Produces("application/json")]
public class PaymentController : ControllerBase
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    private readonly ICheckoutService _checkoutService;
    private readonly IPaymentWebhookService _webhookService;
    private readonly StripeOptions _stripeOptions;

    public PaymentController(
        ICheckoutService checkoutService,
        IPaymentWebhookService webhookService,
        IOptions<StripeOptions> stripeOptions)
    {
        _checkoutService = checkoutService;
        _webhookService  = webhookService;
        _stripeOptions   = stripeOptions.Value;
    }

    /// <summary>
    /// Initialise le paiement par abonnement à partir du panier : crée la commande/abonnements en Pending,
    /// crée le paiement chez Stripe et renvoie le(s) client secret(s) + la clé publiable.
    /// </summary>
    /// <param name="dto">L'adresse de facturation.</param>
    /// <response code="201">Paiement initialisé : client secret(s) renvoyé(s).</response>
    /// <response code="400">Adresse manquante, panier vide ou sans ligne facturable.</response>
    /// <response code="401">Utilisateur non authentifié.</response>
    /// <response code="404">Utilisateur ou plan tarifaire introuvable.</response>
    /// <response code="502">Erreur du fournisseur de paiement.</response>
    [Authorize]
    [HttpPost("subscription")]
    [ProducesResponseType(typeof(CheckoutPaymentResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateSubscriptionPayment([FromBody] CreateCheckoutRequestDto dto)
    {
        if (dto?.Address is null)
            return BadRequest(new { message = "Adresse de facturation requise." });

        try
        {
            var userId = ClaimsHelper.GetUserId(User);
            _logger.Info("POST /payments/subscription — userId={UserId}", userId);

            var result = await _checkoutService.InitSubscriptionPaymentAsync(userId, dto.Address);

            return StatusCode(StatusCodes.Status201Created, new CheckoutPaymentResponseDto
            {
                OrderId         = result.OrderId,
                ClientSecret    = result.ClientSecrets.FirstOrDefault(),
                ClientSecrets   = result.ClientSecrets,
                SubscriptionIds = result.Subscriptions.Select(s => s.StripeSubscriptionId).ToList(),
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

    /// <summary>
    /// Endpoint webhook Stripe (source de vérité des paiements). Vérifie la signature et applique les effets en base.
    /// </summary>
    /// <response code="200">Événement traité (ou ignoré).</response>
    /// <response code="400">Signature invalide ou corps illisible.</response>
    [AllowAnonymous]
    [HttpPost("webhook")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Webhook()
    {
        using var reader = new StreamReader(Request.Body);
        var json = await reader.ReadToEndAsync();
        var signature = Request.Headers["Stripe-Signature"].ToString();

        try
        {
            await _webhookService.HandleEventAsync(json, signature);
            return Ok();
        }
        catch (StripeException ex)
        {
            _logger.Warn(ex, "Webhook Stripe rejeté (signature invalide)");
            return BadRequest();
        }
    }
}
