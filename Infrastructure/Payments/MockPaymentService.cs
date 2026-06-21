namespace Infrastructure.Payments;

using Domain.Dto.Payments;
using Domain.Entities;

using Infrastructure.Interfaces;

using NLog;

using Tools;

/// <summary>
/// Implémentation factice de la passerelle de paiement.
/// Reproduit le comportement historique (faux client secrets / identifiants) sans appel réseau.
/// Active par défaut tant que <c>Payments:Provider</c> n'est pas "Stripe".
/// </summary>
public class MockPaymentService : IPaymentService
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    private readonly IUserRepository _userRepository;

    public MockPaymentService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <inheritdoc />
    public async Task<string> EnsureCustomerAsync(User user)
    {
        if (!string.IsNullOrEmpty(user.StripeCustomerId))
            return user.StripeCustomerId;

        user.StripeCustomerId = $"cus_mock_{Guid.NewGuid():N}";
        await _userRepository.UpdateAsync(user);

        _logger.Info("[MOCK] Client de paiement {CustomerId} créé pour l'utilisateur ID {UserId}",
            user.StripeCustomerId, user.Id);
        return user.StripeCustomerId;
    }

    /// <inheritdoc />
    public async Task<PaymentInitResultDto> CreateSubscriptionPaymentAsync(User user, CreateSubscriptionPaymentRequestDto request)
    {
        var customerId = await EnsureCustomerAsync(user);

        // Une "subscription" factice par ligne récurrente (1:1 avec la Subscription locale).
        var subscriptions = request.Lines
            .Where(l => l.BillingPeriod != BillingPeriod.Lifetime)
            .Select(l => new RecurringPaymentResultDto
            {
                ProductId            = l.ProductId,
                PricingPlanId        = l.PricingPlanId,
                BillingPeriod        = l.BillingPeriod,
                StripeSubscriptionId = $"sub_mock_{Guid.NewGuid():N}",
                ClientSecret         = $"pi_mock_{Guid.NewGuid():N}_secret_mock",
            })
            .ToList();

        string? lifetimePaymentIntentId = null;
        string? lifetimeClientSecret    = null;
        if (request.Lines.Any(l => l.BillingPeriod == BillingPeriod.Lifetime))
        {
            lifetimePaymentIntentId = $"pi_mock_{Guid.NewGuid():N}";
            lifetimeClientSecret    = $"{lifetimePaymentIntentId}_secret_mock";
        }

        _logger.Info("[MOCK] Paiement initialisé pour la commande {OrderId} ({Count} abonnement(s))",
            request.OrderId, subscriptions.Count);

        return new PaymentInitResultDto
        {
            OrderId                 = request.OrderId,
            CustomerId              = customerId,
            Subscriptions           = subscriptions,
            LifetimePaymentIntentId = lifetimePaymentIntentId,
            LifetimeClientSecret    = lifetimeClientSecret,
        };
    }
}
