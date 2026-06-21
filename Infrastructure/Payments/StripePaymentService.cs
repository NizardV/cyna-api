namespace Infrastructure.Payments;

using Domain.Dto.Payments;
using Domain.Entities;

using Infrastructure.Interfaces;

using Microsoft.Extensions.Options;

using NLog;

using Stripe;

/// <summary>
/// Implémentation Stripe de la passerelle de paiement.
/// Active lorsque <c>Payments:Provider</c> vaut "Stripe".
/// La logique métier (Customer + Subscription) est complétée en phase 1.
/// </summary>
public class StripePaymentService : IPaymentService
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    private readonly StripeOptions _options;

    public StripePaymentService(IOptions<StripeOptions> options)
    {
        _options = options.Value;

        // Clé API globale du SDK Stripe.
        StripeConfiguration.ApiKey = _options.SecretKey;
        _logger.Debug("StripePaymentService initialisé (provider=Stripe).");
    }

    /// <inheritdoc />
    public Task<string> EnsureCustomerAsync(User user)
        => throw new NotImplementedException("Implémenté en phase 1 (StripePaymentService).");

    /// <inheritdoc />
    public Task<PaymentInitResultDto> CreateSubscriptionPaymentAsync(User user, CreateSubscriptionPaymentRequestDto request)
        => throw new NotImplementedException("Implémenté en phase 1 (StripePaymentService).");
}
