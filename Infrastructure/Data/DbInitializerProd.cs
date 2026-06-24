using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

using Domain.Entities;
using Domain.Entities.Catalogue;
using Domain.Entities.PromoAndCms;

using Entities;
using Entities.PromoAndCms;

using Tools;

/// <summary>
/// Seeds the production database with clean, real data.
/// No fake data — only admin account, real cybersecurity catalogue, CMS content.
/// Safe to call on every startup — exits immediately if data already exists.
/// </summary>
public static class DbInitializerProd
{
    private static readonly PasswordHasher<object> Hasher = new();

    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Users.AnyAsync()) return;

        // ── 1. Admin account ──────────────────────────────────────────────
        var adminPassword = Environment.GetEnvironmentVariable("ADMIN_SEED_PASSWORD")
            ?? throw new InvalidOperationException("ADMIN_SEED_PASSWORD non défini.");

        var admin = new User
        {
            Email = "admin@projet-cyna.fr",
            FirstName = "Admin",
            LastName = "Cyna",
            PasswordHash = Hasher.HashPassword(null!, adminPassword),
            Role = UserRole.SuperAdmin,
            IsEmailVerified = true,
            CreatedAt = DateTime.UtcNow,
        };
        context.Users.Add(admin);
        await context.SaveChangesAsync();

        // ── 2. Catalogue ──────────────────────────────────────────────────
        SeedCatalogue(context);
        await context.SaveChangesAsync();

        // ── 3. CMS ────────────────────────────────────────────────────────
        SeedCms(context);
        await context.SaveChangesAsync();

        // ── 4. Promo codes ────────────────────────────────────────────────
        SeedPromoCodes(context);
        await context.SaveChangesAsync();
    }

    // ── Catalogue ─────────────────────────────────────────────────────────

    private static void SeedCatalogue(AppDbContext context)
    {
        var categories = new List<(string Slug, string NameFr, string NameEn, string DescFr, string DescEn, string ImageUrl)>
        {
            (
                "soc",
                "SOC",
                "SOC",
                "Centre opérationnel de sécurité pour la surveillance et la réponse aux incidents en temps réel.",
                "Security Operations Center for real-time threat monitoring and incident response.",
                "https://images.unsplash.com/photo-1558494949-ef010cbdcc31?w=800"
            ),
            (
                "edr",
                "EDR",
                "EDR",
                "Détection et réponse aux menaces sur les endpoints pour une protection avancée des postes de travail.",
                "Endpoint Detection and Response for advanced workstation threat protection.",
                "https://images.unsplash.com/photo-1550751827-4bd374c3f58b?w=800"
            ),
            (
                "xdr",
                "XDR",
                "XDR",
                "Détection et réponse étendues couvrant l'ensemble de votre infrastructure IT.",
                "Extended Detection and Response covering your entire IT infrastructure.",
                "https://images.unsplash.com/photo-1563986768609-322da13575f3?w=800"
            ),
            (
                "siem",
                "SIEM",
                "SIEM",
                "Gestion des informations et des événements de sécurité pour une visibilité complète.",
                "Security Information and Event Management for complete visibility.",
                "https://images.unsplash.com/photo-1504868584819-f8e8b4b6d7e3?w=800"
            ),
            (
                "zero-trust",
                "Zero Trust",
                "Zero Trust",
                "Architecture de sécurité basée sur le principe de ne jamais faire confiance, toujours vérifier.",
                "Security architecture based on the principle of never trust, always verify.",
                "https://images.unsplash.com/photo-1555949963-aa79dcee981c?w=800"
            ),
            (
                "mdm",
                "MDM",
                "MDM",
                "Gestion des appareils mobiles pour sécuriser et contrôler les terminaux de votre organisation.",
                "Mobile Device Management to secure and control your organization's endpoints.",
                "https://images.unsplash.com/photo-1512941937669-90a1b58e7e9c?w=800"
            ),
        };

        var products = new List<(string CategorySlug, string Slug, string NameFr, string NameEn, string DescFr, string DescEn, string[] Images, string Specs)>
        {
            // SOC
            (
                "soc",
                "cyna-soc-manager",
                "Cyna SOC Manager",
                "Cyna SOC Manager",
                "Solution complète de gestion de votre centre opérationnel de sécurité. Surveillance 24/7, corrélation d'événements et tableaux de bord en temps réel.",
                "Complete Security Operations Center management solution. 24/7 monitoring, event correlation and real-time dashboards.",
                ["https://images.unsplash.com/photo-1558494949-ef010cbdcc31?w=800",
                 "https://images.unsplash.com/photo-1573164713714-d95e436ab8d4?w=800",
                 "https://images.unsplash.com/photo-1551808525-51a94da548ce?w=800"],
                "Plateformes: Windows, macOS, Linux | SLA: 99.9% uptime | Support: 24/7 | Intégrations: SIEM, EDR, XDR"
            ),
            (
                "soc",
                "sentinel-soc-pro",
                "Sentinel SOC Pro",
                "Sentinel SOC Pro",
                "Plateforme SOC professionnelle avec intelligence artificielle intégrée pour la détection proactive des menaces.",
                "Professional SOC platform with integrated artificial intelligence for proactive threat detection.",
                ["https://images.unsplash.com/photo-1551808525-51a94da548ce?w=800",
                 "https://images.unsplash.com/photo-1558494949-ef010cbdcc31?w=800",
                 "https://images.unsplash.com/photo-1504868584819-f8e8b4b6d7e3?w=800"],
                "Plateformes: Windows, macOS, Linux | SLA: 99.5% uptime | Support: 24/7 | IA: Oui"
            ),
            // EDR
            (
                "edr",
                "cyna-edr-pro",
                "Cyna EDR Pro",
                "Cyna EDR Pro",
                "Solution EDR de pointe pour la détection et la neutralisation des menaces avancées sur vos endpoints.",
                "Cutting-edge EDR solution for detecting and neutralizing advanced threats on your endpoints.",
                ["https://images.unsplash.com/photo-1550751827-4bd374c3f58b?w=800",
                 "https://images.unsplash.com/photo-1563986768609-322da13575f3?w=800",
                 "https://images.unsplash.com/photo-1555949963-aa79dcee981c?w=800"],
                "Plateformes: Windows, macOS, Linux | SLA: 99.9% uptime | Support: 24/7 | MaxDevices: 10000"
            ),
            (
                "edr",
                "guard-edr-suite",
                "Guard EDR Suite",
                "Guard EDR Suite",
                "Suite complète de protection des endpoints avec analyse comportementale et réponse automatisée.",
                "Complete endpoint protection suite with behavioral analysis and automated response.",
                ["https://images.unsplash.com/photo-1563986768609-322da13575f3?w=800",
                 "https://images.unsplash.com/photo-1550751827-4bd374c3f58b?w=800",
                 "https://images.unsplash.com/photo-1573164713714-d95e436ab8d4?w=800"],
                "Plateformes: Windows, macOS, Linux, iOS, Android | SLA: 99.5% uptime | Support: 24/7"
            ),
            // XDR
            (
                "xdr",
                "apex-xdr-suite",
                "Apex XDR Suite",
                "Apex XDR Suite",
                "Solution XDR unifiée offrant une visibilité complète sur l'ensemble de votre infrastructure et une réponse coordonnée aux incidents.",
                "Unified XDR solution offering complete visibility across your entire infrastructure and coordinated incident response.",
                ["https://images.unsplash.com/photo-1573164713714-d95e436ab8d4?w=800",
                 "https://images.unsplash.com/photo-1558494949-ef010cbdcc31?w=800",
                 "https://images.unsplash.com/photo-1550751827-4bd374c3f58b?w=800"],
                "Plateformes: Windows, macOS, Linux | SLA: 99.9% uptime | Support: 24/7 | Cloud: AWS, Azure, GCP"
            ),
            // SIEM
            (
                "siem",
                "cyna-siem-core",
                "Cyna SIEM Core",
                "Cyna SIEM Core",
                "Plateforme SIEM centralisée pour la collecte, l'analyse et la corrélation de tous vos événements de sécurité.",
                "Centralized SIEM platform for collecting, analyzing and correlating all your security events.",
                ["https://images.unsplash.com/photo-1504868584819-f8e8b4b6d7e3?w=800",
                 "https://images.unsplash.com/photo-1573164713714-d95e436ab8d4?w=800",
                 "https://images.unsplash.com/photo-1551808525-51a94da548ce?w=800"],
                "Plateformes: Cloud, On-Premise | SLA: 99.9% uptime | Support: 24/7 | Rétention: 1 an"
            ),
            // Zero Trust
            (
                "zero-trust",
                "cyna-zero-trust-gateway",
                "Cyna Zero Trust Gateway",
                "Cyna Zero Trust Gateway",
                "Passerelle Zero Trust pour sécuriser l'accès à vos applications et données avec une vérification continue de l'identité.",
                "Zero Trust gateway to secure access to your applications and data with continuous identity verification.",
                ["https://images.unsplash.com/photo-1555949963-aa79dcee981c?w=800",
                 "https://images.unsplash.com/photo-1563986768609-322da13575f3?w=800",
                 "https://images.unsplash.com/photo-1504868584819-f8e8b4b6d7e3?w=800"],
                "Plateformes: Cloud-native | SLA: 99.9% uptime | Support: 24/7 | MFA: Oui | SSO: Oui"
            ),
            // MDM
            (
                "mdm",
                "shield-mdm-pro",
                "Shield MDM Pro",
                "Shield MDM Pro",
                "Solution MDM complète pour gérer et sécuriser l'ensemble des appareils mobiles de votre organisation.",
                "Complete MDM solution to manage and secure all mobile devices in your organization.",
                ["https://images.unsplash.com/photo-1512941937669-90a1b58e7e9c?w=800",
                 "https://images.unsplash.com/photo-1555949963-aa79dcee981c?w=800",
                 "https://images.unsplash.com/photo-1550751827-4bd374c3f58b?w=800"],
                "Plateformes: iOS, Android, Windows Mobile | SLA: 99.5% uptime | Support: 24/7 | MaxDevices: 50000"
            ),
        };

        var categoryEntities = new Dictionary<string, Category>();

        for (int i = 0; i < categories.Count; i++)
        {
            var c = categories[i];
            var cat = new Category
            {
                Slug = c.Slug,
                ImageUrl = c.ImageUrl,
                DisplayOrder = i,
            };
            cat.Translations.Add(new CategoryTranslation { Locale = LocaleLang.Fr, Name = c.NameFr, Description = c.DescFr });
            cat.Translations.Add(new CategoryTranslation { Locale = LocaleLang.En, Name = c.NameEn, Description = c.DescEn });
            context.Categories.Add(cat);
            categoryEntities[c.Slug] = cat;
        }

        for (int i = 0; i < products.Count; i++)
        {
            var p = products[i];
            var cat = categoryEntities[p.CategorySlug];

            var product = new Product
            {
                Category = cat,
                Slug = p.Slug,
                TechnicalSpecs = p.Specs,
                Status = ProductStatus.Available,
                IsFeatured = i < 3,
                DisplayOrder = i,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            product.Translations.Add(new ProductTranslation { Locale = LocaleLang.Fr, Name = p.NameFr, Description = p.DescFr });
            product.Translations.Add(new ProductTranslation { Locale = LocaleLang.En, Name = p.NameEn, Description = p.DescEn });

            for (int img = 0; img < p.Images.Length; img++)
                product.Images.Add(new ProductImage { ImageUrl = p.Images[img], DisplayOrder = img });

            // Plan mensuel
            var monthly = new PricingPlan
            {
                Product = product,
                Name = "Mensuel",
                BillingPeriod = BillingPeriod.Monthly,
                DiscountPercent = 0,
            };
            monthly.PricingTiers.Add(new PricingTier { unitType = BillingUnit.User, minQuantity = 1, maxQuantity = 10, PricePerUnit = 15.00m });
            monthly.PricingTiers.Add(new PricingTier { unitType = BillingUnit.User, minQuantity = 11, maxQuantity = 50, PricePerUnit = 10.00m });
            monthly.PricingTiers.Add(new PricingTier { unitType = BillingUnit.Device, minQuantity = 1, maxQuantity = 25, PricePerUnit = 8.00m });
            monthly.PricingTiers.Add(new PricingTier { unitType = BillingUnit.Device, minQuantity = 26, maxQuantity = 100, PricePerUnit = 5.00m });

            // Plan annuel
            var yearly = new PricingPlan
            {
                Product = product,
                Name = "Annuel",
                BillingPeriod = BillingPeriod.Yearly,
                DiscountPercent = 17,
            };
            yearly.PricingTiers.Add(new PricingTier { unitType = BillingUnit.User, minQuantity = 1, maxQuantity = 10, PricePerUnit = 150.00m });
            yearly.PricingTiers.Add(new PricingTier { unitType = BillingUnit.User, minQuantity = 11, maxQuantity = 50, PricePerUnit = 100.00m });
            yearly.PricingTiers.Add(new PricingTier { unitType = BillingUnit.Device, minQuantity = 1, maxQuantity = 25, PricePerUnit = 80.00m });
            yearly.PricingTiers.Add(new PricingTier { unitType = BillingUnit.Device, minQuantity = 26, maxQuantity = 100, PricePerUnit = 50.00m });

            product.PricingPlans.Add(monthly);
            product.PricingPlans.Add(yearly);
            context.Products.Add(product);
        }
    }

    // ── CMS ───────────────────────────────────────────────────────────────

    private static void SeedCms(AppDbContext context)
    {
        var slides = new[]
        {
            ("https://images.unsplash.com/photo-1558494949-ef010cbdcc31?w=1200",
             "Protégez votre infrastructure", "Des solutions SaaS de cybersécurité de pointe pour votre entreprise.", "Découvrir",
             "Protect your infrastructure", "Cutting-edge SaaS cybersecurity solutions for your business.", "Discover"),
            ("https://images.unsplash.com/photo-1550751827-4bd374c3f58b?w=1200",
             "Détection avancée des menaces", "EDR, XDR et SOC unifiés pour une protection complète.", "En savoir plus",
             "Advanced Threat Detection", "Unified EDR, XDR and SOC for complete protection.", "Learn more"),
            ("https://images.unsplash.com/photo-1555949963-aa79dcee981c?w=1200",
             "Zero Trust Security", "Ne faites jamais confiance, vérifiez toujours. Sécurisez chaque accès.", "Commencer",
             "Zero Trust Security", "Never trust, always verify. Secure every access point.", "Get started"),
            ("https://images.unsplash.com/photo-1504868584819-f8e8b4b6d7e3?w=1200",
             "Conformité et visibilité", "SIEM centralisé pour une conformité totale et une visibilité complète.", "Voir les solutions",
             "Compliance and visibility", "Centralized SIEM for full compliance and complete visibility.", "View solutions"),
        };

        for (int i = 0; i < slides.Length; i++)
        {
            var (img, titleFr, subtitleFr, btnFr, titleEn, subtitleEn, btnEn) = slides[i];
            var slide = new CarouselSlide
            {
                ImageUrl = img,
                DisplayOrder = i,
                IsActive = true,
            };
            slide.Translations.Add(new CarouselSlideTranslation { Locale = LocaleLang.Fr, Title = titleFr, Subtitle = subtitleFr, ButtonText = btnFr });
            slide.Translations.Add(new CarouselSlideTranslation { Locale = LocaleLang.En, Title = titleEn, Subtitle = subtitleEn, ButtonText = btnEn });
            context.CarouselSlides.Add(slide);
        }

        var mission = new SiteSetting { SettingKey = "homepage_mission_text" };
        mission.Translations.Add(new SiteSettingTranslation { Locale = LocaleLang.Fr, Setting = mission, SettingValue = "Cyna protège vos entreprises grâce à des solutions SaaS de cybersécurité de pointe, accessibles et évolutives." });
        mission.Translations.Add(new SiteSettingTranslation { Locale = LocaleLang.En, Setting = mission, SettingValue = "Cyna protects your business with cutting-edge, accessible and scalable SaaS cybersecurity solutions." });
        context.SiteSettings.Add(mission);
    }

    // ── Promo codes ───────────────────────────────────────────────────────

    private static void SeedPromoCodes(AppDbContext context)
    {
        context.PromoCodes.AddRange(
            new PromoCode { Code = "WELCOME10", DiscountPercent = 10, IsActive = true, ExpiresAt = DateTime.UtcNow.AddMonths(6) },
            new PromoCode { Code = "CYBER20", DiscountPercent = 20, IsActive = true, ExpiresAt = DateTime.UtcNow.AddMonths(3) }
        );
    }
}