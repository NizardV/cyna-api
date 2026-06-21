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

        // Stripe crée un paiement (et un abonnement) par intervalle de facturation.
        var groups = request.Lines.GroupBy(l => l.BillingPeriod).ToList();

        var clientSecrets = groups
            .Select(_ => $"pi_mock_{Guid.NewGuid():N}_secret_mock")
            .ToList();

        var subscriptionIds = groups
            .Where(g => g.Key != BillingPeriod.Lifetime)
            .Select(_ => $"sub_mock_{Guid.NewGuid():N}")
            .ToList();

        _logger.Info("[MOCK] Paiement initialisé pour l'utilisateur ID {UserId} ({Count} paiement(s))",
            user.Id, clientSecrets.Count);

        return new PaymentInitResultDto
        {
            CustomerId      = customerId,
            ClientSecrets   = clientSecrets,
            SubscriptionIds = subscriptionIds,
        };
    }
}
