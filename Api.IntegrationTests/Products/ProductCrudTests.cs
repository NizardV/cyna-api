using System.Net;
using System.Net.Http.Json;

using Api.IntegrationTests.Auth;

using Domain.Dto.Product;
using Domain.Entities;
using Domain.Entities.AddressAndPayment;
using Domain.Entities.Catalogue;
using Domain.Entities.OrdersAndSubscriptions;

using Infrastructure.Data;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using Tools;

namespace Api.IntegrationTests.Products;

/// <summary>
/// Tests d'intégration du CRUD produit du back-office (/products).
/// </summary>
public class ProductCrudTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private const string AdminRole = "Administrateur";
    private const string SimpleUserRole = "Utilisateur";

    private readonly CustomWebApplicationFactory<Program> _factory;

    public ProductCrudTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    /// <summary>Crée un client HTTP, anonyme par défaut ou porteur du rôle donné.</summary>
    private HttpClient CreateClient(string? role = null)
    {
        // Base https pour neutraliser UseHttpsRedirection dans les tests
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

        if (role != null)
        {
            client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, role);
        }

        return client;
    }

    /// <summary>Insère une catégorie valide directement en base et retourne son identifiant.</summary>
    private async Task<int> SeedCategoryAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var category = new Category { Slug = $"cat-{Guid.NewGuid():N}" };
        category.Translations.Add(new CategoryTranslation { Locale = LocaleLang.Fr, Name = "Catégorie de test" });

        db.Categories.Add(category);
        await db.SaveChangesAsync();

        return category.Id;
    }

    /// <summary>Construit un payload de création valide au format envoyé par le formulaire admin.</summary>
    private static object BuildValidPayload(int categoryId, string nameFr)
    {
        return new
        {
            nameFr,
            nameEn = "Test Product EN",
            descriptionFr = "Description française",
            descriptionEn = "English description",
            status = "Available",
            categoryId,
            imageUrl = "https://example.com/image.png",
            isFeatured = true,
            displayOrder = 3,
            technicalSpecs = new[] { "Protection multi-terminaux", "Support 24/7" },
            pricingPlans = new[]
            {
                new
                {
                    name = "Mensuel",
                    billingPeriod = "monthly",
                    discountPercent = 0,
                    maxUsersCheckout = 10,
                    maxDevicesCheckout = 100,
                    pricingTiers = new[]
                    {
                        new { unitType = "user",   minQty = 1, maxQty = 5,  unitPrice = 49.99 },
                        new { unitType = "device", minQty = 1, maxQty = 50, unitPrice = 5.99 }
                    }
                }
            }
        };
    }

    /// <summary>Crée un produit via l'API admin et retourne le DTO résultant.</summary>
    private async Task<ProductAdminDto> CreateProductViaApiAsync(string nameFr)
    {
        var categoryId = await SeedCategoryAsync();
        var client = CreateClient(AdminRole);

        var response = await client.PostAsJsonAsync("/products", BuildValidPayload(categoryId, nameFr));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<ProductAdminDto>();
        Assert.NotNull(created);

        return created;
    }

    // -------------------------------------------------------------------------
    // Sécurité
    // -------------------------------------------------------------------------

    [Fact]
    public async Task CreateProduct_SansAuthentification_Renvoie401()
    {
        var client = CreateClient();

        var response = await client.PostAsJsonAsync("/products", BuildValidPayload(1, "Produit anonyme"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_AvecRoleUtilisateur_Renvoie403()
    {
        var client = CreateClient(SimpleUserRole);

        var response = await client.PostAsJsonAsync("/products", BuildValidPayload(1, "Produit interdit"));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetProducts_SansAuthentification_Renvoie401()
    {
        var client = CreateClient();

        var response = await client.GetAsync("/products");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteProduct_SansAuthentification_Renvoie401()
    {
        var client = CreateClient();

        var response = await client.DeleteAsync("/products/1");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // -------------------------------------------------------------------------
    // Création
    // -------------------------------------------------------------------------

    [Fact]
    public async Task CreateProduct_PayloadValide_Renvoie201AvecSlugEtPlans()
    {
        var nameFr = $"Cyna EDR Pro {Guid.NewGuid():N}";
        var created = await CreateProductViaApiAsync(nameFr);

        Assert.Equal(nameFr, created.NameFr);
        Assert.Equal("Test Product EN", created.NameEn);
        Assert.Equal("Available", created.Status);
        Assert.True(created.IsFeatured);
        Assert.Equal(3, created.DisplayOrder);
        Assert.StartsWith("cyna-edr-pro-", created.Slug);
        Assert.Equal("https://example.com/image.png", created.ImageUrl);
        Assert.Equal(2, created.TechnicalSpecs.Count());

        var plan = Assert.Single(created.PricingPlans);
        Assert.Equal("monthly", plan.BillingPeriod);
        Assert.Equal(10, plan.MaxUsersCheckout);
        Assert.Equal(100, plan.MaxDevicesCheckout);
        Assert.Equal(2, plan.PricingTiers.Count());

        var userTier = plan.PricingTiers.First(t => t.UnitType == "user");
        Assert.Equal(1, userTier.MinQty);
        Assert.Equal(5, userTier.MaxQty);
        Assert.Equal(49.99m, userTier.UnitPrice);
    }

    [Fact]
    public async Task CreateProduct_StatutInconnu_Renvoie400()
    {
        var categoryId = await SeedCategoryAsync();
        var client = CreateClient(AdminRole);

        var payload = new
        {
            nameFr = "Produit statut invalide",
            status = "EnRupture",
            categoryId,
            technicalSpecs = Array.Empty<string>(),
            pricingPlans = Array.Empty<object>()
        };

        var response = await client.PostAsJsonAsync("/products", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_CategorieInexistante_Renvoie400()
    {
        var client = CreateClient(AdminRole);

        var payload = new
        {
            nameFr = "Produit sans catégorie",
            status = "Available",
            categoryId = 999_999,
            technicalSpecs = Array.Empty<string>(),
            pricingPlans = Array.Empty<object>()
        };

        var response = await client.PostAsJsonAsync("/products", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_SansNomFrancais_Renvoie400()
    {
        var categoryId = await SeedCategoryAsync();
        var client = CreateClient(AdminRole);

        var payload = new
        {
            nameFr = "",
            status = "Available",
            categoryId,
            technicalSpecs = Array.Empty<string>(),
            pricingPlans = Array.Empty<object>()
        };

        var response = await client.PostAsJsonAsync("/products", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_NomIdentique_GenereUnSlugUnique()
    {
        var categoryId = await SeedCategoryAsync();
        var client = CreateClient(AdminRole);
        var nameFr = $"Sentinel SOC {Guid.NewGuid():N}";

        var first = await client.PostAsJsonAsync("/products", BuildValidPayload(categoryId, nameFr));
        var second = await client.PostAsJsonAsync("/products", BuildValidPayload(categoryId, nameFr));

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Created, second.StatusCode);

        var firstDto = await first.Content.ReadFromJsonAsync<ProductAdminDto>();
        var secondDto = await second.Content.ReadFromJsonAsync<ProductAdminDto>();

        Assert.NotEqual(firstDto!.Slug, secondDto!.Slug);
        Assert.Equal($"{firstDto.Slug}-2", secondDto.Slug);
    }

    // -------------------------------------------------------------------------
    // Lecture
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetProducts_EnAdmin_ContientLeProduitCree()
    {
        var nameFr = $"Apex SIEM {Guid.NewGuid():N}";
        var created = await CreateProductViaApiAsync(nameFr);

        var client = CreateClient(AdminRole);
        var products = await client.GetFromJsonAsync<List<ProductAdminListItemDto>>("/products");

        Assert.NotNull(products);
        var item = Assert.Single(products.Where(p => p.Id == created.Id));
        Assert.Equal(nameFr, item.Name);
        Assert.Equal("Available", item.Status);
        Assert.True(item.IsFeatured);
    }

    [Fact]
    public async Task GetProductAdmin_RenvoieLesDeuxLocales()
    {
        var nameFr = $"Guard XDR {Guid.NewGuid():N}";
        var created = await CreateProductViaApiAsync(nameFr);

        var client = CreateClient(AdminRole);
        var product = await client.GetFromJsonAsync<ProductAdminDto>($"/products/{created.Id}/admin");

        Assert.NotNull(product);
        Assert.Equal(nameFr, product.NameFr);
        Assert.Equal("Test Product EN", product.NameEn);
        Assert.Equal("Description française", product.DescriptionFr);
        Assert.Equal("English description", product.DescriptionEn);
    }

    [Fact]
    public async Task GetProductAdmin_Inexistant_Renvoie404()
    {
        var client = CreateClient(AdminRole);

        var response = await client.GetAsync("/products/999999/admin");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetProductDetails_Public_RenvoieLesSpecsEnTableau()
    {
        var nameFr = $"Shield Zero Trust {Guid.NewGuid():N}";
        var created = await CreateProductViaApiAsync(nameFr);

        // La fiche produit publique est accessible sans authentification
        var client = CreateClient();
        var product = await client.GetFromJsonAsync<ProductDetailsDto>($"/products/{created.Id}");

        Assert.NotNull(product);
        Assert.Equal(nameFr, product.Name);
        Assert.Equal("available", product.Status); // La fiche publique expose le statut en minuscules
        Assert.Equal(2, product.TechnicalSpecs.Count());
        Assert.Contains("Support 24/7", product.TechnicalSpecs);
    }

    [Fact]
    public async Task GetCategories_Public_RenvoieLaListe()
    {
        var categoryId = await SeedCategoryAsync();

        var client = CreateClient();
        var response = await client.GetAsync("/categories?locale=fr");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var categories = await response.Content.ReadFromJsonAsync<List<Domain.Dto.Catalog.CategoryDto>>();
        Assert.NotNull(categories);
        Assert.Contains(categories, c => c.Id == categoryId);
    }

    // -------------------------------------------------------------------------
    // Mise à jour
    // -------------------------------------------------------------------------

    [Fact]
    public async Task UpdateProduct_ChangeNomStatutEtPlans()
    {
        var created = await CreateProductViaApiAsync($"Produit à modifier {Guid.NewGuid():N}");
        var client = CreateClient(AdminRole);

        var newNameFr = $"Produit modifié {Guid.NewGuid():N}";
        var payload = new
        {
            nameFr = newNameFr,
            nameEn = "Updated Product EN",
            descriptionFr = "Nouvelle description",
            descriptionEn = "New description",
            status = "Unavailable",
            categoryId = created.CategoryId,
            imageUrl = "https://example.com/new-image.png",
            isFeatured = false,
            technicalSpecs = new[] { "Spec unique" },
            pricingPlans = new[]
            {
                new
                {
                    name = "Annuel",
                    billingPeriod = "yearly",
                    discountPercent = 15,
                    maxUsersCheckout = 20,
                    maxDevicesCheckout = 200,
                    pricingTiers = new[]
                    {
                        new { unitType = "user", minQty = 1, maxQty = 10, unitPrice = 39.99 }
                    }
                }
            }
        };

        var response = await client.PutAsJsonAsync($"/products/{created.Id}", payload);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updated = await response.Content.ReadFromJsonAsync<ProductAdminDto>();
        Assert.NotNull(updated);
        Assert.Equal(newNameFr, updated.NameFr);
        Assert.Equal("Unavailable", updated.Status);
        Assert.False(updated.IsFeatured);
        Assert.Null(updated.DisplayOrder);
        Assert.Equal("https://example.com/new-image.png", updated.ImageUrl);
        Assert.Equal(created.Slug, updated.Slug); // Le slug ne change pas en mise à jour

        // Le plan mensuel (sans référence de commande) a été remplacé par l'annuel
        var plan = Assert.Single(updated.PricingPlans);
        Assert.Equal("yearly", plan.BillingPeriod);
        Assert.Equal(15, plan.DiscountPercent);

        var tier = Assert.Single(plan.PricingTiers);
        Assert.Equal(39.99m, tier.UnitPrice);
    }

    [Fact]
    public async Task UpdateProduct_Inexistant_Renvoie404()
    {
        var categoryId = await SeedCategoryAsync();
        var client = CreateClient(AdminRole);

        var response = await client.PutAsJsonAsync("/products/999999", BuildValidPayload(categoryId, "Produit fantôme"));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // -------------------------------------------------------------------------
    // Suppression
    // -------------------------------------------------------------------------

    [Fact]
    public async Task DeleteProduct_SansReference_Renvoie204PuisDisparait()
    {
        var created = await CreateProductViaApiAsync($"Produit à supprimer {Guid.NewGuid():N}");
        var client = CreateClient(AdminRole);

        var deleteResponse = await client.DeleteAsync($"/products/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await client.GetAsync($"/products/{created.Id}/admin");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteProduct_Inexistant_Renvoie404()
    {
        var client = CreateClient(AdminRole);

        var response = await client.DeleteAsync("/products/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteProduct_ReferenceParUneCommande_Renvoie409()
    {
        var created = await CreateProductViaApiAsync($"Produit commandé {Guid.NewGuid():N}");
        var planId = created.PricingPlans.First().Id;

        // Une commande payée référence le produit : la suppression doit être bloquée
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var user = new User
            {
                Email = $"client-{Guid.NewGuid():N}@test.fr",
                PasswordHash = "hash",
                FirstName = "Jean",
                LastName = "Testeur"
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var address = new Address
            {
                UserId = user.Id,
                FirstName = "Jean",
                LastName = "Testeur",
                AddressLine1 = "1 rue du Test",
                City = "Lyon",
                PostalCode = "69000",
                Country = "FR"
            };
            db.Addresses.Add(address);
            await db.SaveChangesAsync();

            var order = new Order
            {
                UserId = user.Id,
                BillingAddressId = address.Id,
                Status = OrderStatus.Paid,
                TotalAmount = 49.99m
            };
            order.Items.Add(new OrderItem
            {
                ProductId = created.Id,
                PricingPlanId = planId,
                ProductNameSnapshot = created.NameFr,
                PlanNameSnapshot = "Mensuel",
                QuantityUsers = 1,
                QuantityDevices = 0,
                UnitPriceUsers = 49.99m,
                UnitPriceDevices = 0m
            });
            db.Orders.Add(order);
            await db.SaveChangesAsync();
        }

        var client = CreateClient(AdminRole);
        var response = await client.DeleteAsync($"/products/{created.Id}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        // Le produit est toujours là
        var getResponse = await client.GetAsync($"/products/{created.Id}/admin");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    }
}
