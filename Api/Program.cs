using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

using Api.Interceptors;

using Application.Interfaces;
using Application.Interfaces.Services;
using Application.Services;

using Infrastructure.Data;
using Infrastructure.Interfaces;
using Infrastructure.Repositories;
using Infrastructure.Security;

using NLog;
using NLog.Web;

var builder = WebApplication.CreateBuilder(args);
// Initiation du logger NLog pour la classe courante afin de pouvoir l'utiliser pour logger des messages d'information, d'erreur, etc avant la construction de l'application.
var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Debug("init main");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
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

    // D�finition du sch�ma d�authentification Bearer
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

    // Exigence de s�curit� � NOUVELLE SYNTAXE .NET 10 / Swashbuckle 10
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        // La cl� doit utiliser **exactement** le m�me nom que dans AddSecurityDefinition
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

builder.Services.AddSingleton<EfSlowQueryInterceptor>();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
    options.UseSqlite(connectionString).AddInterceptors(serviceProvider.GetRequiredService<EfSlowQueryInterceptor>()));

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
    });

builder.Services.AddAuthorization();

// DI m�tiers
// --- Dépôts (Infrastructure → Domain) ---
builder.Services.AddScoped<IUserRepository,         UserRepository>();
builder.Services.AddScoped<IOrderRepository,        OrderRepository>();
builder.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
builder.Services.AddScoped<ICatalogRepository,      CatalogRepository>();
builder.Services.AddScoped<ICarouselRepository, CarouselRepository>();
builder.Services.AddScoped<ISiteSettingRepository, SiteSettingRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>(); 

// --- Services (Application) ---
builder.Services.AddScoped<IUserService,         UserService>();
builder.Services.AddScoped<IOrderService,        OrderService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<ICatalogService,      CatalogService>();
builder.Services.AddScoped<ICmsService, CmsService>();


// Hasher de mot de passe
builder.Services.AddSingleton<IPasswordHasher, IdentityPasswordHasher>();

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
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();