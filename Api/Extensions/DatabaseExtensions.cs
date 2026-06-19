namespace Api.Extensions;

using Api.Interceptors;

using Infrastructure.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

public static class DatabaseExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services,
        IConfiguration config, IWebHostEnvironment env)
    {
        services.AddSingleton<EfSlowQueryInterceptor>();

        var provider = config["DatabaseProvider"] ?? "sqlite";

        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetRequiredService<EfSlowQueryInterceptor>());

            if (provider.Equals("postgres", StringComparison.OrdinalIgnoreCase))
                options.UseNpgsql(config.GetConnectionString("DefaultConnection"));
            else
                options.UseSqlite(config.GetConnectionString("DefaultConnection"));

            options.ConfigureWarnings(w =>
                w.Ignore(RelationalEventId.PendingModelChangesWarning));
        });

        return services;
    }
}