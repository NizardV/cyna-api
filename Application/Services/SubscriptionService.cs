using Application.Dtos.User;
using Application.Interfaces.Services;
using Domain.Repositories;
using NLog;

namespace Application.Services;

/// <summary>
/// Service de gestion des abonnements utilisateur.
/// Mappe les entités de la base de données vers les DTOs de l'API.
/// </summary>
public class SubscriptionService : ISubscriptionService
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    private readonly ISubscriptionRepository _subscriptionRepository;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="SubscriptionService"/>.
    /// </summary>
    /// <param name="subscriptionRepository">Le dépôt des abonnements.</param>
    public SubscriptionService(ISubscriptionRepository subscriptionRepository)
    {
        _subscriptionRepository = subscriptionRepository;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SubscriptionDto>> GetUserSubscriptionsAsync(int userId)
    {
        _logger.Info("Récupération des abonnements pour l'utilisateur ID {UserId}", userId);

        var subscriptions = await _subscriptionRepository.GetByUserIdAsync(userId);

        return subscriptions.Select(s =>
        {
            // Récupère le nom du produit dans la langue française par défaut
            var productName = s.Product.Translations
                .FirstOrDefault()?.Name ?? s.Product.Slug;

            return new SubscriptionDto
            {
                Id                  = s.Id,
                Status              = s.Status.ToString(),
                ProductName         = productName,
                PlanName            = s.PricingPlan.Name,
                CurrentPeriodStart  = s.CurrentPeriodStart,
                CurrentPeriodEnd    = s.CurrentPeriodEnd,
                AutoRenew           = s.AutoRenew,
            };
        });
    }
}