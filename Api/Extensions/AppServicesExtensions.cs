namespace Api.Extensions;

using Application.Interfaces;
using Application.Interfaces.Services;
using Application.Services;

using Infrastructure.Interfaces;
using Infrastructure.Payments;
using Infrastructure.Repositories;

using Resend;

using Tools;

public static class AppServicesExtensions
{
    public static IServiceCollection AddAppServices(this IServiceCollection services,
        IConfiguration config)
    {
        // --- Repositories ---
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<ICatalogRepository, CatalogRepository>();
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<ICarouselRepository, CarouselRepository>();
        services.AddScoped<ISiteSettingRepository, SiteSettingRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ISearchRepository, SearchRepository>();
        services.AddScoped<IDashboardRepository, DashboardRepository>();

        // --- Services ---
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<ISubscriptionService, SubscriptionService>();
        services.AddScoped<ICatalogService, CatalogService>();
        services.AddScoped<AuthService>();
        services.AddScoped<IAuthService>(sp => sp.GetRequiredService<AuthService>());
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ICmsService, CmsService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ISearchService, SearchService>();
        services.AddScoped<IDashboardService, DashboardService>();

        // --- Paiement (bascule Mock / Stripe via Payments:Provider) ---
        services.Configure<PaymentOptions>(config.GetSection("Payments"));
        services.Configure<StripeOptions>(config.GetSection("Stripe"));

        var paymentProvider = config["Payments:Provider"] ?? "Mock";
        if (paymentProvider.Equals("Stripe", StringComparison.OrdinalIgnoreCase))
            services.AddScoped<IPaymentService, StripePaymentService>();
        else
            services.AddScoped<IPaymentService, MockPaymentService>();

        services.AddScoped<ICheckoutService, CheckoutService>();
        services.AddScoped<IPaymentWebhookService, PaymentWebhookService>();
        services.AddScoped<Api.Helpers.StripeTestHelper>();

        // --- Email ---
        services.AddOptions();
        services.AddHttpClient<ResendClient>();
        services.Configure<ResendClientOptions>(o => o.ApiToken = config["Resend:ApiKey"]!);
        services.AddTransient<IResend, ResendClient>();
        services.AddTransient<EmailHelper>();

        services.AddHttpContextAccessor();

        return services;
    }
}