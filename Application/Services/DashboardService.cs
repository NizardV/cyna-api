using Bogus;

using Infrastructure.Entities;
using Infrastructure.Interfaces;

using NLog;

namespace Application.Services;

using Domain.Dto.Dashboard;

using Interfaces;

/// <summary>
/// Service de statistiques du dashboard admin.
/// Délègue au <see cref="IDashboardRepository"/> pour les données réelles, ou génère des
/// données factices via Bogus quand <c>mock=true</c> (voir remarque sur <see cref="RevenueStatsDto"/>).
/// </summary>
public class DashboardService : IDashboardService
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    // Graine fixe : des appels successifs avec ?mock=true sur la même période renvoient
    // des montants cohérents entre les routes (CA, commandes, produits) pendant une démo.
    private const int MockSeed = 2026;

    private static readonly string[] MockProductNames =
    [
        "Cyna EDR Pro", "Shield XDR Suite", "Guard SOC Manager",
        "Sentinel Zero Trust Gateway", "Apex SIEM Core", "Cyna MDM Lite",
    ];

    private readonly IDashboardRepository _repository;

    public DashboardService(IDashboardRepository repository)
    {
        _repository = repository;
    }

    // -------------------------------------------------------------------------
    // CA
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public async Task<RevenueStatsDto> GetRevenueStatsAsync(DashboardFilterDto filter, bool mock)
    {
        var (start, end) = filter.Resolve();

        if (mock)
        {
            _logger.Info("Dashboard CA — mode mock");
            return BuildMockRevenueStats(start, end);
        }

        var span          = end - start;
        var previousStart = start - span;
        var previousEnd   = start;

        return await _repository.GetRevenueStatsAsync(start, end, previousStart, previousEnd);
    }

    private static RevenueStatsDto BuildMockRevenueStats(DateTime start, DateTime end)
    {
        var faker     = new Faker { Random = new Randomizer(MockSeed) };
        var byMonth   = GenerateMonthlyRange(start, end);
        var revenueByMonth = byMonth
            .Select(m => new MonthlyRevenueDto
            {
                Year    = m.Year,
                Month   = m.Month,
                Revenue = Math.Round(faker.Random.Decimal(2_000, 15_000), 2),
            })
            .ToList();

        var currentPeriod  = revenueByMonth.Sum(m => m.Revenue);
        var previousPeriod = Math.Round(faker.Random.Decimal(2_000, 15_000), 2);
        var total           = currentPeriod + Math.Round(faker.Random.Decimal(50_000, 200_000), 2);

        var growthPercent = previousPeriod == 0m
            ? 0m
            : Math.Round((currentPeriod - previousPeriod) / previousPeriod * 100m, 2);

        return new RevenueStatsDto
        {
            Total          = total,
            CurrentPeriod  = currentPeriod,
            PreviousPeriod = previousPeriod,
            GrowthPercent  = growthPercent,
            ByMonth        = revenueByMonth,
        };
    }

    // -------------------------------------------------------------------------
    // Orders
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public async Task<OrderStatsDto> GetOrderStatsAsync(DashboardFilterDto filter, bool mock)
    {
        var (start, end) = filter.Resolve();

        if (mock)
        {
            _logger.Info("Dashboard commandes — mode mock");
            return BuildMockOrderStats(start, end);
        }

        return await _repository.GetOrderStatsAsync(start, end);
    }

    private static OrderStatsDto BuildMockOrderStats(DateTime start, DateTime end)
    {
        var faker   = new Faker { Random = new Randomizer(MockSeed) };
        var byMonth = GenerateMonthlyRange(start, end)
            .Select(m => new MonthlyOrderCountDto
            {
                Year  = m.Year,
                Month = m.Month,
                Count = faker.Random.Int(10, 80),
            })
            .ToList();

        var total = byMonth.Sum(m => m.Count);

        // Répartition approximative par statut (clés en minuscules, alignées sur l'enum OrderStatus côté C#).
        var byStatus = new Dictionary<string, int>
        {
            ["pending"]   = (int)(total * 0.10),
            ["paid"]      = (int)(total * 0.65),
            ["failed"]    = (int)(total * 0.05),
            ["refunded"]  = (int)(total * 0.05),
            ["cancelled"] = (int)(total * 0.15),
        };

        return new OrderStatsDto
        {
            Total    = total,
            ByStatus = byStatus,
            ByMonth  = byMonth,
        };
    }

    // -------------------------------------------------------------------------
    // Users
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public async Task<UserStatsDto> GetUserStatsAsync(DashboardFilterDto filter, bool mock)
    {
        var (start, end) = filter.Resolve();

        if (mock)
        {
            _logger.Info("Dashboard utilisateurs — mode mock");
            return BuildMockUserStats(start, end);
        }

        return await _repository.GetUserStatsAsync(start, end);
    }

    private static UserStatsDto BuildMockUserStats(DateTime start, DateTime end)
    {
        var faker   = new Faker { Random = new Randomizer(MockSeed) };
        var byMonth = GenerateMonthlyRange(start, end)
            .Select(m => new MonthlyUserCountDto
            {
                Year  = m.Year,
                Month = m.Month,
                Count = faker.Random.Int(5, 40),
            })
            .ToList();

        var newInPeriod = byMonth.Sum(m => m.Count);
        var total        = newInPeriod + faker.Random.Int(200, 1000);

        return new UserStatsDto
        {
            Total         = total,
            NewInPeriod   = newInPeriod,
            VerifiedEmail = (int)(total * 0.85),
            ByMonth       = byMonth,
        };
    }

    // -------------------------------------------------------------------------
    // Subscriptions
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public async Task<SubscriptionStatsDto> GetSubscriptionStatsAsync(DashboardFilterDto filter, bool mock)
    {
        var (start, end) = filter.Resolve();

        if (mock)
        {
            _logger.Info("Dashboard abonnements — mode mock");
            return BuildMockSubscriptionStats(start, end);
        }

        return await _repository.GetSubscriptionStatsAsync(start, end);
    }

    private static SubscriptionStatsDto BuildMockSubscriptionStats(DateTime start, DateTime end)
    {
        var faker   = new Faker { Random = new Randomizer(MockSeed) };
        var byMonth = GenerateMonthlyRange(start, end)
            .Select(m => new MonthlySubscriptionCountDto
            {
                Year  = m.Year,
                Month = m.Month,
                Count = faker.Random.Int(8, 50),
            })
            .ToList();

        var total = byMonth.Sum(m => m.Count);

        // Clés en minuscules, alignées sur l'enum SubscriptionStatus côté C#.
        var byStatus = new Dictionary<string, int>
        {
            ["active"]    = (int)(total * 0.55),
            ["cancelled"] = (int)(total * 0.15),
            ["expired"]   = (int)(total * 0.15),
            ["suspended"] = (int)(total * 0.05),
            ["pending"]   = (int)(total * 0.10),
        };

        return new SubscriptionStatsDto
        {
            Total    = total,
            Active   = byStatus["active"],
            ByStatus = byStatus,
            ByMonth  = byMonth,
        };
    }

    // -------------------------------------------------------------------------
    // Top products
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public async Task<IEnumerable<TopProductDto>> GetTopProductsAsync(
        DashboardFilterDto filter, TopProductSortBy sortBy, int limit, bool mock)
    {
        var (start, end) = filter.Resolve();

        if (mock)
        {
            _logger.Info("Dashboard top produits — mode mock");
            return BuildMockTopProducts(sortBy, limit);
        }

        return await _repository.GetTopProductsAsync(start, end, sortBy, limit);
    }

    private static IEnumerable<TopProductDto> BuildMockTopProducts(TopProductSortBy sortBy, int limit)
    {
        var faker = new Faker { Random = new Randomizer(MockSeed) };

        var products = MockProductNames
            .Select((name, i) => new TopProductDto
            {
                ProductId   = i + 1,
                ProductName = name,
                ImageUrl    = $"https://picsum.photos/seed/{name.Replace(' ', '-').ToLowerInvariant()}/400/300",
                Revenue     = Math.Round(faker.Random.Decimal(1_000, 25_000), 2),
                OrdersCount = faker.Random.Int(5, 200),
            });

        products = sortBy == TopProductSortBy.Orders
            ? products.OrderByDescending(p => p.OrdersCount)
            : products.OrderByDescending(p => p.Revenue);

        return products.Take(limit).ToList();
    }
}