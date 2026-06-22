using Infrastructure.Payments;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using NLog;

using Stripe;

namespace Api.Controllers;

using Domain.Dto.Payments;

using ILogger = NLog.ILogger;

/// <summary>
/// Routes de TEST de paiement (mode Développement uniquement, sans authentification).
/// Créent ET confirment le paiement côté serveur avec une carte de test Stripe choisie dans le body,
/// puis renvoient directement le résultat (réussi / refusé / 3DS). Pour tester vite plusieurs cartes
/// depuis Swagger, sans panier ni connexion. Ne crée aucune commande locale.
/// </summary>
[ApiController]
[Route("payments/test")]
[Produces("application/json")]
[AllowAnonymous]
public class PaymentTestController : ControllerBase
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    private readonly StripeOptions _stripeOptions;
    private readonly IWebHostEnvironment _env;

    public PaymentTestController(IOptions<StripeOptions> stripeOptions, IWebHostEnvironment env)
    {
        _stripeOptions = stripeOptions.Value;
        _env           = env;
    }

    /// <summary>Abonnement de test (1 €/mois) payé immédiatement avec la carte fournie.</summary>
    /// <remarks>Body : { "amountCents": 100, "paymentMethod": "pm_card_visa" }</remarks>
    /// <response code="200">Résultat du paiement (réussi / refusé / 3DS).</response>
    /// <response code="404">Route indisponible (hors développement).</response>
    [HttpPost("subscription")]
    public async Task<IActionResult> TestSubscription([FromBody] PaymentTestRequestDto? dto)
    {
        if (!_env.IsDevelopment()) return NotFound();
        dto ??= new PaymentTestRequestDto();
        StripeConfiguration.ApiKey = _stripeOptions.SecretKey;

        var customer = await new CustomerService().CreateAsync(new CustomerCreateOptions
        {
            Email = $"test+{Guid.NewGuid():N}@cyna.fr",
        });

        var product = await new ProductService().CreateAsync(new ProductCreateOptions
        {
            Name = "Test abonnement Cyna",
        });

        var subOptions = new SubscriptionCreateOptions
        {
            Customer = customer.Id,
            Items =
            [
                new SubscriptionItemOptions
                {
                    PriceData = new SubscriptionItemPriceDataOptions
                    {
                        Currency   = "eur",
                        Product    = product.Id,
                        UnitAmount = dto.AmountCents,
                        Recurring  = new SubscriptionItemPriceDataRecurringOptions { Interval = "month" },
                    },
                    Quantity = 1,
                },
            ],
            PaymentBehavior = "default_incomplete",
            PaymentSettings = new SubscriptionPaymentSettingsOptions
            {
                SaveDefaultPaymentMethod = "on_subscription",
                PaymentMethodTypes       = ["card"],
            },
            Metadata = new Dictionary<string, string> { ["test"] = "true" },
        };
        subOptions.AddExpand("latest_invoice.confirmation_secret");

        var subscription = await new SubscriptionService().CreateAsync(subOptions);
        var paymentIntentId = PaymentIntentIdFrom(subscription.LatestInvoice?.ConfirmationSecret?.ClientSecret);

        try
        {
            var pi = await new PaymentIntentService().ConfirmAsync(paymentIntentId, new PaymentIntentConfirmOptions
            {
                PaymentMethod = dto.PaymentMethod,
                ReturnUrl     = "https://example.com/return",
            });
            return Ok(SuccessResult(pi, dto, subscription.Id));
        }
        catch (StripeException ex)
        {
            _logger.Warn(ex, "Paiement test abonnement refusé (carte {Card})", dto.PaymentMethod);
            return Ok(DeclineResult(ex, dto, subscription.Id));
        }
    }

    /// <summary>Achat unique de test (1 €) payé immédiatement avec la carte fournie.</summary>
    /// <remarks>Body : { "amountCents": 100, "paymentMethod": "pm_card_visa" }</remarks>
    /// <response code="200">Résultat du paiement (réussi / refusé / 3DS).</response>
    /// <response code="404">Route indisponible (hors développement).</response>
    [HttpPost("one-time")]
    public async Task<IActionResult> TestOneTime([FromBody] PaymentTestRequestDto? dto)
    {
        if (!_env.IsDevelopment()) return NotFound();
        dto ??= new PaymentTestRequestDto();
        StripeConfiguration.ApiKey = _stripeOptions.SecretKey;

        var customer = await new CustomerService().CreateAsync(new CustomerCreateOptions
        {
            Email = $"test+{Guid.NewGuid():N}@cyna.fr",
        });

        try
        {
            var pi = await new PaymentIntentService().CreateAsync(new PaymentIntentCreateOptions
            {
                Amount                  = dto.AmountCents,
                Currency                = "eur",
                Customer                = customer.Id,
                PaymentMethod           = dto.PaymentMethod,
                Confirm                 = true,
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled        = true,
                    AllowRedirects = "never",
                },
                Metadata = new Dictionary<string, string> { ["test"] = "true" },
            });
            return Ok(SuccessResult(pi, dto, subscriptionId: null));
        }
        catch (StripeException ex)
        {
            _logger.Warn(ex, "Paiement test achat refusé (carte {Card})", dto.PaymentMethod);
            return Ok(DeclineResult(ex, dto, subscriptionId: null));
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static object SuccessResult(PaymentIntent pi, PaymentTestRequestDto dto, string? subscriptionId)
    {
        var label = pi.Status switch
        {
            "succeeded"        => "✅ Paiement réussi",
            "requires_action"  => "🔐 Authentification 3D Secure requise (à finaliser côté front)",
            _                  => $"⚠️ Statut : {pi.Status}",
        };

        return new
        {
            result          = label,
            status          = pi.Status,
            amount          = $"{dto.AmountCents / 100m:0.00} EUR",
            card            = dto.PaymentMethod,
            paymentIntentId = pi.Id,
            subscriptionId,
        };
    }

    private static object DeclineResult(StripeException ex, PaymentTestRequestDto dto, string? subscriptionId) => new
    {
        result      = "❌ Paiement refusé",
        status      = "declined",
        amount      = $"{dto.AmountCents / 100m:0.00} EUR",
        card        = dto.PaymentMethod,
        declineCode = ex.StripeError?.DeclineCode,
        errorCode   = ex.StripeError?.Code,
        message     = ex.StripeError?.Message ?? ex.Message,
        subscriptionId,
    };

    /// <summary>Extrait l'identifiant du PaymentIntent (pi_...) à partir d'un client secret (pi_..._secret_...).</summary>
    private static string PaymentIntentIdFrom(string? clientSecret)
    {
        if (string.IsNullOrEmpty(clientSecret)) return string.Empty;
        var idx = clientSecret.IndexOf("_secret_", StringComparison.Ordinal);
        return idx > 0 ? clientSecret[..idx] : clientSecret;
    }
}
