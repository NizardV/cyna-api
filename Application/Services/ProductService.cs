using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

using Application.Interfaces.Services;

using Domain.Dto.Product;
using Domain.Entities.Catalogue;

using Infrastructure.Interfaces;

using Microsoft.Extensions.Logging;

using Tools;

namespace Application.Services;

/// <summary>
/// Implémentation du service produits.
/// Gère la consultation, la création, la mise à jour et la suppression des produits
/// ainsi que le mapping vers les DTOs publics et back-office.
/// </summary>
public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<ProductService> _logger;

    /// <summary>
    /// Initialise une nouvelle instance de <see cref="ProductService"/>.
    /// </summary>
    /// <param name="productRepository">Le dépôt produits.</param>
    /// <param name="logger">Le logger.</param>
    public ProductService(IProductRepository productRepository, ILogger<ProductService> logger)
    {
        _productRepository = productRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<ProductDetailsDto?> GetProductDetailsAsync(int id, LocaleLang locale)
    {
        _logger.LogInformation("Récupération isolée des détails du produit Id={Id} (Locale: {Locale})", id, locale);

        var product = await _productRepository.GetProductDetailsByIdAsync(id, locale);

        if (product == null)
        {
            _logger.LogWarning("Produit Id={Id} introuvable en base.", id);
            return null;
        }

        var pTranslation = product.Translations.FirstOrDefault();
        var cTranslation = product.Category?.Translations.FirstOrDefault();

        return new ProductDetailsDto
        {
            Id = product.Id,
            Slug = product.Slug,
            Name = pTranslation?.Name ?? product.Slug,
            Description = pTranslation?.Description ?? string.Empty,
            TechnicalSpecs = DeserializeSpecs(product.TechnicalSpecs),
            // Les pages publiques comparent le statut en minuscules (comme les produits similaires)
            Status = product.Status?.ToString().ToLowerInvariant() ?? string.Empty,

            Category = new ProductCategoryDto
            {
                Id = product.Category!.Id,
                Slug = product.Category.Slug,
                Name = cTranslation?.Name ?? product.Category.Slug,
                Description = cTranslation?.Description,
                ImageUrl = product.Category.ImageUrl
            },

            Images = product.Images.Select(i => i.ImageUrl).ToList(),

            PricingPlans = product.PricingPlans.Select(pp => new ProductPricingPlanDto
            {
                Id = pp.Id,
                Name = pp.Name,
                BillingPeriod = pp.BillingPeriod.ToString().ToLowerInvariant(),
                DiscountPercent = pp.DiscountPercent,
                MaxUsersCheckout = pp.MaxUsersCheckout,
                MaxDevicesCheckout = pp.MaxDevicesCheckout,
                PricingTiers = pp.PricingTiers.Select(t => new ProductPricingTierDto
                {
                    UnitType = t.unitType.ToString().ToLowerInvariant(),
                    MinQty = t.minQuantity,
                    MaxQty = t.maxQuantity,
                    UnitPrice = t.PricePerUnit
                }).ToList()
            }).ToList()
        };
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ProductSimilarDto>> GetSimilarProductsAsync(int currentProductId, LocaleLang locale)
    {
        _logger.LogInformation("Récupération des produits similaires pour le produit Id={Id} (Locale: {Locale})", currentProductId, locale);

        var products = await _productRepository.GetSimilarProductsAsync(currentProductId, locale);

        return products.Select(p =>
        {
            var translation = p.Translations.FirstOrDefault();
            var image = p.Images.FirstOrDefault();

            decimal? price = p.PricingPlans
                .SelectMany(plan => plan.PricingTiers)
                .Select(tier => (decimal?)tier.PricePerUnit)
                .DefaultIfEmpty()
                .Min();

            string desc = translation?.Description ?? string.Empty;
            if (desc.Length > 100)
            {
                desc = desc.Substring(0, 97) + "...";
            }

            return new ProductSimilarDto
            {
                Id = p.Id,
                Slug = p.Slug,
                Name = translation?.Name ?? p.Slug,
                Description = desc,
                Status = p.Status.ToString().ToLower(),
                ImageUrl = image?.ImageUrl,
                Price = price == 0 ? null : price
            };
        }).ToList();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ProductAdminListItemDto>> GetProductsForAdminAsync()
    {
        _logger.LogInformation("Récupération de la liste des produits pour le back-office");

        var products = await _productRepository.GetAllForAdminAsync();

        return products.Select(p =>
        {
            var translation = p.Translations.FirstOrDefault(t => t.Locale == LocaleLang.Fr)
                           ?? p.Translations.FirstOrDefault();

            return new ProductAdminListItemDto
            {
                Id = p.Id,
                Name = translation?.Name ?? p.Slug,
                Description = translation?.Description ?? string.Empty,
                Status = p.Status?.ToString() ?? string.Empty,
                CategoryId = p.CategoryId,
                ImageUrl = p.Images.FirstOrDefault()?.ImageUrl,
                IsFeatured = p.IsFeatured,
                DisplayOrder = p.DisplayOrder,
                CreatedAt = p.CreatedAt
            };
        }).ToList();
    }

    /// <inheritdoc />
    public async Task<ProductAdminDto?> GetProductForAdminAsync(int id)
    {
        _logger.LogInformation("Récupération du produit Id={Id} pour le back-office", id);

        var product = await _productRepository.GetAdminDetailsByIdAsync(id);
        return product == null ? null : MapToAdminDto(product);
    }

    /// <inheritdoc />
    public async Task<ProductAdminDto> CreateProductAsync(ProductUpsertRequestDto dto)
    {
        await ValidateUpsertAsync(dto);
        var status = ParseStatus(dto.Status);

        var product = new Product
        {
            CategoryId = dto.CategoryId,
            Slug = await GenerateUniqueSlugAsync(dto.NameFr),
            TechnicalSpecs = SerializeSpecs(dto.TechnicalSpecs),
            Status = status,
            IsFeatured = dto.IsFeatured,
            DisplayOrder = dto.IsFeatured ? dto.DisplayOrder : null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        product.Translations.Add(new ProductTranslation
        {
            Locale = LocaleLang.Fr,
            Name = dto.NameFr.Trim(),
            Description = dto.DescriptionFr?.Trim() ?? string.Empty
        });

        if (!string.IsNullOrWhiteSpace(dto.NameEn))
        {
            product.Translations.Add(new ProductTranslation
            {
                Locale = LocaleLang.En,
                Name = dto.NameEn.Trim(),
                Description = dto.DescriptionEn?.Trim() ?? string.Empty
            });
        }

        if (!string.IsNullOrWhiteSpace(dto.ImageUrl))
        {
            product.Images.Add(new ProductImage { ImageUrl = dto.ImageUrl.Trim(), DisplayOrder = 0 });
        }

        foreach (var planDto in dto.PricingPlans)
        {
            product.PricingPlans.Add(BuildPlan(planDto));
        }

        await _productRepository.AddAsync(product);

        _logger.LogInformation("Produit créé Id={Id}, Slug={Slug}", product.Id, product.Slug);
        return MapToAdminDto(product);
    }

    /// <inheritdoc />
    public async Task<ProductAdminDto?> UpdateProductAsync(int id, ProductUpsertRequestDto dto)
    {
        var product = await _productRepository.GetEditableByIdAsync(id);
        if (product == null)
        {
            _logger.LogWarning("Mise à jour impossible : produit Id={Id} introuvable", id);
            return null;
        }

        await ValidateUpsertAsync(dto);

        product.Status = ParseStatus(dto.Status);
        product.CategoryId = dto.CategoryId;
        product.TechnicalSpecs = SerializeSpecs(dto.TechnicalSpecs);
        product.IsFeatured = dto.IsFeatured;
        product.DisplayOrder = dto.IsFeatured ? dto.DisplayOrder : null;
        product.UpdatedAt = DateTime.UtcNow;
        // Le slug est volontairement conservé pour ne pas casser les liens existants.

        UpsertTranslation(product, LocaleLang.Fr, dto.NameFr, dto.DescriptionFr);

        if (!string.IsNullOrWhiteSpace(dto.NameEn))
        {
            UpsertTranslation(product, LocaleLang.En, dto.NameEn, dto.DescriptionEn);
        }
        else
        {
            foreach (var translation in product.Translations.Where(t => t.Locale == LocaleLang.En).ToList())
            {
                product.Translations.Remove(translation);
            }
        }

        UpsertMainImage(product, dto.ImageUrl);
        await UpsertPricingPlansAsync(product, dto.PricingPlans);

        await _productRepository.SaveChangesAsync();

        _logger.LogInformation("Produit mis à jour Id={Id}", id);
        return MapToAdminDto(product);
    }

    /// <inheritdoc />
    public async Task DeleteProductAsync(int id)
    {
        var product = await _productRepository.GetEditableByIdAsync(id);
        if (product == null)
        {
            throw new KeyNotFoundException($"Le produit avec l'ID {id} n'existe pas.");
        }

        if (await _productRepository.HasOrderOrSubscriptionReferencesAsync(id))
        {
            throw new InvalidOperationException(
                "Ce produit est référencé par des commandes ou des abonnements et ne peut pas être supprimé. " +
                "Passez-le en statut 'Unavailable' pour le retirer du catalogue.");
        }

        await _productRepository.DeleteAsync(product);
        _logger.LogInformation("Produit supprimé Id={Id}, Slug={Slug}", id, product.Slug);
    }

    // -------------------------------------------------------------------------
    // Validation et parsing
    // -------------------------------------------------------------------------

    private async Task ValidateUpsertAsync(ProductUpsertRequestDto dto)
    {
        ParseStatus(dto.Status);

        if (!await _productRepository.CategoryExistsAsync(dto.CategoryId))
        {
            throw new ArgumentException($"La catégorie {dto.CategoryId} n'existe pas.");
        }

        var duplicatedPeriod = dto.PricingPlans
            .GroupBy(p => ParseBillingPeriod(p.BillingPeriod))
            .FirstOrDefault(g => g.Count() > 1);

        if (duplicatedPeriod != null)
        {
            throw new ArgumentException(
                $"Plusieurs plans tarifaires utilisent la même période '{duplicatedPeriod.Key}'. Un seul plan par période est autorisé.");
        }

        foreach (var plan in dto.PricingPlans)
        {
            foreach (var tier in plan.PricingTiers)
            {
                ParseBillingUnit(tier.UnitType);

                if (tier.MaxQty < tier.MinQty)
                {
                    throw new ArgumentException(
                        $"Palier invalide sur le plan '{plan.Name}' : la quantité max ({tier.MaxQty}) doit être supérieure ou égale à la quantité min ({tier.MinQty}).");
                }
            }
        }
    }

    private static ProductStatus ParseStatus(string status)
    {
        if (Enum.TryParse<ProductStatus>(status, ignoreCase: true, out var parsed) && Enum.IsDefined(parsed))
        {
            return parsed;
        }

        throw new ArgumentException(
            $"Statut invalide : '{status}'. Valeurs attendues : {string.Join(", ", Enum.GetNames<ProductStatus>())}.");
    }

    private static BillingPeriod ParseBillingPeriod(string billingPeriod)
    {
        if (Enum.TryParse<BillingPeriod>(billingPeriod, ignoreCase: true, out var parsed) && Enum.IsDefined(parsed))
        {
            return parsed;
        }

        throw new ArgumentException(
            $"Période de facturation invalide : '{billingPeriod}'. Valeurs attendues : {string.Join(", ", Enum.GetNames<BillingPeriod>())}.");
    }

    private static BillingUnit ParseBillingUnit(string unitType)
    {
        if (Enum.TryParse<BillingUnit>(unitType, ignoreCase: true, out var parsed) && Enum.IsDefined(parsed))
        {
            return parsed;
        }

        throw new ArgumentException(
            $"Type d'unité invalide : '{unitType}'. Valeurs attendues : {string.Join(", ", Enum.GetNames<BillingUnit>())}.");
    }

    // -------------------------------------------------------------------------
    // Slug
    // -------------------------------------------------------------------------

    private async Task<string> GenerateUniqueSlugAsync(string name, int? excludeProductId = null)
    {
        var baseSlug = GenerateSlug(name);
        if (baseSlug.Length == 0)
        {
            baseSlug = "produit";
        }

        // Marge pour le suffixe d'unicité (la colonne est limitée à 200 caractères)
        if (baseSlug.Length > 190)
        {
            baseSlug = baseSlug[..190].Trim('-');
        }

        var slug = baseSlug;
        var suffix = 2;

        while (await _productRepository.SlugExistsAsync(slug, excludeProductId))
        {
            slug = $"{baseSlug}-{suffix++}";
        }

        return slug;
    }

    private static string GenerateSlug(string name)
    {
        var withoutDiacritics = new string(name.Trim().ToLowerInvariant()
            .Normalize(NormalizationForm.FormD)
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            .ToArray())
            .Normalize(NormalizationForm.FormC);

        return Regex.Replace(withoutDiacritics, "[^a-z0-9]+", "-").Trim('-');
    }

    // -------------------------------------------------------------------------
    // Spécifications techniques (colonne texte ⇄ liste JSON)
    // -------------------------------------------------------------------------

    private static string? SerializeSpecs(List<string> specs)
    {
        var cleaned = specs
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim())
            .ToList();

        return cleaned.Count > 0 ? JsonSerializer.Serialize(cleaned) : null;
    }

    private static List<string> DeserializeSpecs(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(raw) ?? [];
        }
        catch (JsonException)
        {
            // Données legacy stockées en texte libre séparé par « | » avant la migration JSON
            return raw.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        }
    }

    // -------------------------------------------------------------------------
    // Upserts du graphe produit
    // -------------------------------------------------------------------------

    private static void UpsertTranslation(Product product, LocaleLang locale, string name, string? description)
    {
        var existing = product.Translations.FirstOrDefault(t => t.Locale == locale);

        if (existing == null)
        {
            product.Translations.Add(new ProductTranslation
            {
                Locale = locale,
                Name = name.Trim(),
                Description = description?.Trim() ?? string.Empty
            });
        }
        else
        {
            existing.Name = name.Trim();
            existing.Description = description?.Trim() ?? string.Empty;
        }
    }

    private static void UpsertMainImage(Product product, string? imageUrl)
    {
        var mainImage = product.Images.OrderBy(i => i.DisplayOrder).FirstOrDefault();

        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            if (mainImage != null)
            {
                product.Images.Remove(mainImage);
            }
        }
        else if (mainImage == null)
        {
            product.Images.Add(new ProductImage { ImageUrl = imageUrl.Trim(), DisplayOrder = 0 });
        }
        else
        {
            mainImage.ImageUrl = imageUrl.Trim();
        }
    }

    private async Task UpsertPricingPlansAsync(Product product, List<ProductPricingPlanInputDto> plans)
    {
        var incomingByPeriod = plans.ToDictionary(p => ParseBillingPeriod(p.BillingPeriod));

        // Suppression des plans retirés du formulaire (si non référencés par l'historique)
        foreach (var existing in product.PricingPlans.ToList())
        {
            if (incomingByPeriod.ContainsKey(existing.BillingPeriod))
            {
                continue;
            }

            if (await _productRepository.PlanHasOrderOrSubscriptionReferencesAsync(existing.Id))
            {
                throw new InvalidOperationException(
                    $"Le plan '{existing.Name}' est référencé par des commandes ou des abonnements et ne peut pas être supprimé.");
            }

            product.PricingPlans.Remove(existing);
        }

        // Mise à jour des plans conservés / ajout des nouveaux
        foreach (var (period, planDto) in incomingByPeriod)
        {
            var existing = product.PricingPlans.FirstOrDefault(p => p.BillingPeriod == period);

            if (existing == null)
            {
                product.PricingPlans.Add(BuildPlan(planDto));
                continue;
            }

            existing.Name = planDto.Name.Trim();
            existing.DiscountPercent = planDto.DiscountPercent;
            existing.MaxUsersCheckout = planDto.MaxUsersCheckout;
            existing.MaxDevicesCheckout = planDto.MaxDevicesCheckout;

            // Les paliers n'ont aucune référence externe : remplacement complet
            // Les paliers n'ont aucune référence externe (commandes/abonnements) : remplacement complet autorisé.
            existing.PricingTiers.Clear();
            foreach (var tierDto in planDto.PricingTiers)
            {
                existing.PricingTiers.Add(BuildTier(tierDto));
            }
        }
    }

    private static PricingPlan BuildPlan(ProductPricingPlanInputDto dto)
    {
        return new PricingPlan
        {
            Name = dto.Name.Trim(),
            BillingPeriod = ParseBillingPeriod(dto.BillingPeriod),
            DiscountPercent = dto.DiscountPercent,
            MaxUsersCheckout = dto.MaxUsersCheckout,
            MaxDevicesCheckout = dto.MaxDevicesCheckout,
            PricingTiers = dto.PricingTiers.Select(BuildTier).ToList()
        };
    }

    private static PricingTier BuildTier(ProductPricingTierInputDto dto)
    {
        return new PricingTier
        {
            unitType = ParseBillingUnit(dto.UnitType),
            minQuantity = dto.MinQty,
            maxQuantity = dto.MaxQty,
            PricePerUnit = dto.UnitPrice
        };
    }

    // -------------------------------------------------------------------------
    // Mapping admin
    // -------------------------------------------------------------------------

    private static ProductAdminDto MapToAdminDto(Product product)
    {
        var fr = product.Translations.FirstOrDefault(t => t.Locale == LocaleLang.Fr);
        var en = product.Translations.FirstOrDefault(t => t.Locale == LocaleLang.En);

        return new ProductAdminDto
        {
            Id = product.Id,
            Slug = product.Slug,
            NameFr = fr?.Name ?? string.Empty,
            NameEn = en?.Name ?? string.Empty,
            DescriptionFr = fr?.Description ?? string.Empty,
            DescriptionEn = en?.Description ?? string.Empty,
            Status = product.Status?.ToString() ?? string.Empty,
            CategoryId = product.CategoryId,
            ImageUrl = product.Images.OrderBy(i => i.DisplayOrder).FirstOrDefault()?.ImageUrl,
            IsFeatured = product.IsFeatured,
            DisplayOrder = product.DisplayOrder,
            TechnicalSpecs = DeserializeSpecs(product.TechnicalSpecs),
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt,
            PricingPlans = product.PricingPlans.Select(pp => new ProductAdminPricingPlanDto
            {
                Id = pp.Id,
                Name = pp.Name,
                BillingPeriod = pp.BillingPeriod.ToString().ToLowerInvariant(),
                DiscountPercent = pp.DiscountPercent,
                MaxUsersCheckout = pp.MaxUsersCheckout,
                MaxDevicesCheckout = pp.MaxDevicesCheckout,
                PricingTiers = pp.PricingTiers.OrderBy(t => t.minQuantity).Select(t => new ProductAdminPricingTierDto
                {
                    UnitType = t.unitType.ToString().ToLowerInvariant(),
                    MinQty = t.minQuantity,
                    MaxQty = t.maxQuantity,
                    UnitPrice = t.PricePerUnit
                }).ToList()
            }).ToList()
        };
    }
}