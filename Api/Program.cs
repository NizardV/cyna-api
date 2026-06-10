using System.Text;

using Api.Interceptors;

using Application.Interfaces;

using Scalar.AspNetCore;

using Application.Services;

using Infrastructure.Data;
using Infrastructure.Interfaces;
using Infrastructure.Repositories;

using Application.Interfaces.Services;

using Infrastructure.Security;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

using NLog;
using NLog.Web;

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Initiation du logger NLog pour la classe courante afin de pouvoir l'utiliser pour logger des messages d'information, d'erreur, etc avant la construction de l'application.
    var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
    logger.Debug("init main");

    builder.Services.Configure<CookiePolicyOptions>(options =>
    {
        options.CheckConsentNeeded = context => false;
        options.MinimumSameSitePolicy = SameSiteMode.None;
    });

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("Frontend", policy => policy
            .WithOrigins("http://localhost:5173", "https://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
    });

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddOpenApi();
    builder.Services.AddSwaggerGen(options =>
    {
        // Document de base
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "CynaApi API",
            Version = "v1"
        });

        // GESTION DES CONFLITS DE NOMS DE DTO (Ex: Home.CategoryDto vs Catalog.CategoryDto)
        options.CustomSchemaIds(type => type.FullName);

        // Définition du schéma d'authentification Bearer
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Description = "JWT Authorization header using the Bearer scheme. " +
                          "Exemple : \"Bearer 12345abcdef\"",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        });

        // Exigence de sécurité à NOUVELLE SYNTAXE .NET 10 / Swashbuckle 10
        options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
        {
            // La clé doit utiliser **exactement** le même nom que dans AddSecurityDefinition
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        });
    });

    var apiDocs = builder.Configuration["ApiDocs"] ?? "Scalar";

    builder.Services.AddSingleton<EfSlowQueryInterceptor>();
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
    {
        // Ajoute l'intercepteur commun
        options.AddInterceptors(serviceProvider.GetRequiredService<EfSlowQueryInterceptor>());

        // Si on est en local (Development), on peut rester sur SQLite
        if (builder.Environment.IsDevelopment())
        {
            options.UseSqlite(connectionString);
        }
        else
        {
            // En Staging et Production (sur OVH), on utilise PostgreSQL
            options.UseNpgsql(connectionString);
        }

        // Fix: décalage de version EF tools (10.0.7) vs runtime (10.0.8)
        options.ConfigureWarnings(warnings =>
            warnings.Ignore(RelationalEventId.PendingModelChangesWarning));

    });

    // Jwt options
    builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

    // Authentification / JWT
    var jwtConfig = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()!;
    var keyBytes = Encoding.UTF8.GetBytes(jwtConfig.Key);

    builder.Services
        .AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtConfig.Issuer,
                ValidAudience = jwtConfig.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                ClockSkew = TimeSpan.Zero
            };
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var token = context.Request.Cookies["cyna_token"];
                    if (!string.IsNullOrEmpty(token))
                    {
                        context.Token = token;
                    }
                    return Task.CompletedTask;
                }
            };
        });

    builder.Services.AddAuthorization();

    // DI métiers
    // --- Dépôts (Infrastructure → Domain) ---
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IOrderRepository, OrderRepository>();
    builder.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
    builder.Services.AddScoped<ICatalogRepository, CatalogRepository>();
    builder.Services.AddScoped<ICartRepository, CartRepository>();
    builder.Services.AddScoped<ICarouselRepository, CarouselRepository>();
    builder.Services.AddScoped<ISiteSettingRepository, SiteSettingRepository>();
    builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
    builder.Services.AddScoped<IProductRepository, ProductRepository>();

    // --- Services (Application) ---
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<IOrderService, OrderService>();
    builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
    builder.Services.AddScoped<ICatalogService, CatalogService>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<ICartService, CartService>();
    builder.Services.AddScoped<ICmsService, CmsService>();

    // --- Auth utilisateur ---
    builder.Services.AddHttpContextAccessor();

    // Générateur de Token JWT
    builder.Services.AddSingleton<ITokenGenerator, JwtTokenGenerator>();

    // Hasher de mot de passe

    var app = builder.Build();
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();

        if (args.Contains("--seed") && !app.Environment.IsProduction())
        {
            await DbInitializer.SeedAsync(context);
        }
    }
    if (!app.Environment.IsProduction())
    {
        app.MapOpenApi();

        if (apiDocs.Equals("Swagger", StringComparison.OrdinalIgnoreCase))
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                options.RoutePrefix = string.Empty;
            });
        }
        else
        {
            app.MapScalarApiReference();
        }
    }

    // Middleware ordonnées correctement (ordre à respecter pour que les cookies fonctionnent)

    // 1. Redirection HTTP vers HTTPS
    app.UseHttpsRedirection();

    // 2. Gestion du CORS (Indispensable pour le proxy Vite)
    app.UseCors("Frontend");

    // 3. Application de la politique des cookies
    app.UseCookiePolicy();

    // 4. Authentification (Extrait les tokens/cookies)
    app.UseAuthentication();

    // 5. Autorisation (Vérifie les accès [Authorize])
    app.UseAuthorization();

    // 6. Mapping des routes
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    // Log l'exception fatale qui empêche l'application de démarrer
    var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
    logger.Fatal(ex, "Application failed to start");
    throw;
}