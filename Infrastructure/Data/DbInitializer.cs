using Bogus;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

using Bogus.DataSets;

using Domain.Entities;
using Domain.Entities.AddressAndPayment;
using Domain.Entities.Catalogue;
using Domain.Entities.OrdersAndSubscriptions;
using Domain.Entities.PromoAndCms;

using Entities;
using Entities.PromoAndCms;

using Address = Domain.Entities.AddressAndPayment.Address;

/// <summary>
/// Seeds the database with realistic fake data using the Bogus library.
/// Mirrors the JS factories.js structure (makeUser, makeCategory, makeProduct…).
/// Only runs when the DB is empty — safe to call on every startup in Development.
/// </summary>
public static class DbInitializer
{
    // ── Constants matching factories.js ────────────────────────────────────
    private static readonly string[] CategoryNames   = ["SOC", "EDR", "XDR", "SIEM", "Zero Trust", "MDM"];
    private static readonly string[] ProductPrefixes = ["Cyna", "Shield", "Guard", "Sentinel", "Apex"];
    private static readonly string[] ProductSuffixes = ["EDR Pro", "XDR Suite", "SOC Manager", "Zero Trust Gateway", "SIEM Core"];
    private static readonly OrderStatus[]        OrderStatuses  = [OrderStatus.Pending, OrderStatus.Paid, OrderStatus.Cancelled, OrderStatus.Refunded, OrderStatus.Failed];
    private static readonly SubscriptionStatus[] SubStatuses    = [SubscriptionStatus.Active, SubscriptionStatus.Cancelled, SubscriptionStatus.Expired, SubscriptionStatus.Pending, SubscriptionStatus.Suspended];
    private static readonly BillingPeriod[]      BillingPeriods = [BillingPeriod.Lifetime, BillingPeriod.Monthly, BillingPeriod.Yearly];
    private static readonly BillingUnit[]        BillingUnits   = [BillingUnit.Device, BillingUnit.User];
    private static readonly CardBrand[]          CardBrands     = [CardBrand.Mastercard, CardBrand.Visa];
    private static readonly LocaleLang[]         Locales        = [LocaleLang.En, LocaleLang.Fr];

    private static readonly PasswordHasher<object> Hasher = new();

    // ── Entry point ────────────────────────────────────────────────────────

    public static async Task SeedAsync(AppDbContext context)
    {
        // Nothing to do if any table already has rows
        if (await context.Users.AnyAsync()) return;

        Randomizer.Seed = new Random(42); // reproducible seed for demos

        // ── 1. Users ───────────────────────────────────────────────────────
        var users = SeedUsers(context);
        await context.SaveChangesAsync();

        // ── 2. Addresses & Payment methods ────────────────────────────────
        SeedAddressesAndPayments(context, users);
        await context.SaveChangesAsync();

        // ── 3. Catalogue: categories → products → plans → tiers → images ──
        var (categories, products, plans) = SeedCatalogue(context);
        await context.SaveChangesAsync();

        // ── 4. CMS: carousel + site settings ──────────────────────────────
        SeedCms(context);
        await context.SaveChangesAsync();

        // ── 5. Promos ──────────────────────────────────────────────────────
        var promos = SeedPromoCodes(context);
        await context.SaveChangesAsync();

        // ── 6. Cart items (a few per user) ────────────────────────────────
        SeedCartItems(context, users, products, plans);
        await context.SaveChangesAsync();

        // ── 7. Subscriptions → Orders → Order items → Invoices ───────────
        SeedOrdersAndSubscriptions(context, users, products, plans, promos);
        await context.SaveChangesAsync();

        // ── 8. Support: contact messages + chatbot ────────────────────────
        SeedSupport(context, users);
        await context.SaveChangesAsync();
    }

    // ── 1. Users ───────────────────────────────────────────────────────────

    private static List<User> SeedUsers(AppDbContext context)
    {
        // Fixed accounts always present
        var fixed_ = new List<User>
        {
            MakeUser("admin@cyna.fr",      "Admin123!",  UserRole.Admin),
            MakeUser("superadmin@cyna.fr", "Super123!",  UserRole.SuperAdmin),
            MakeUser("user@cyna.fr",       "User123!",   UserRole.User),
        };

        // Random users — mirrors makeUser()
        var faker = new Faker<User>("fr")
            .RuleFor(u => u.Email,           f => f.Internet.Email())
            .RuleFor(u => u.FirstName,       f => f.Name.FirstName())
            .RuleFor(u => u.LastName,        f => f.Name.LastName())
            .RuleFor(u => u.PasswordHash,    f => Hasher.HashPassword(null!, "Password1!"))
            .RuleFor(u => u.Role,            _ => UserRole.User)
            .RuleFor(u => u.IsEmailVerified, _ => true)
            .RuleFor(u => u.CreatedAt,       f => f.Date.Past(2).ToUniversalTime());

        var random = faker.Generate(12);

        var all = fixed_.Concat(random).ToList();
        context.Users.AddRange(all);
        return all;
    }

    private static User MakeUser(string email, string plainPassword, UserRole role) => new()
    {
        Email           = email,
        FirstName       = role == UserRole.Admin || role == UserRole.SuperAdmin ? "Admin" : "User",
        LastName        = "Cyna",
        PasswordHash    = Hasher.HashPassword(null!, plainPassword),
        Role            = role,
        IsEmailVerified = true,
        CreatedAt       = DateTime.UtcNow,
    };

    // ── 2. Addresses & Payment methods ────────────────────────────────────

    private static void SeedAddressesAndPayments(AppDbContext context, List<User> users)
    {
        var addressFaker = new Faker<Address>("fr")
            .RuleFor(a => a.FirstName,    f => f.Name.FirstName())
            .RuleFor(a => a.LastName,     f => f.Name.LastName())
            .RuleFor(a => a.AddressLine1, f => f.Address.StreetAddress())
            .RuleFor(a => a.AddressLine2, f => f.Random.Bool(0.3f) ? f.Address.SecondaryAddress() : null)
            .RuleFor(a => a.City,         f => f.Address.City())
            .RuleFor(a => a.Region,       f => f.Address.State())
            .RuleFor(a => a.PostalCode,   f => f.Address.ZipCode())
            .RuleFor(a => a.Country,      f => f.Address.CountryCode())
            .RuleFor(a => a.Phone,        f => f.Phone.PhoneNumber())
            .RuleFor(a => a.IsDefault,    _ => false);

        var pmFaker = new Faker<PaymentMethod>()
            .RuleFor(p => p.StripePaymentMethodId, f => $"pm_{f.Random.AlphaNumeric(24)}")
            .RuleFor(p => p.CardBrand,             f => f.PickRandom(CardBrands))
            .RuleFor(p => p.CardLast4,             f => f.Finance.CreditCardNumber(CardType.Visa)[^4..])
            .RuleFor(p => p.IsDefault,             _ => false);

        foreach (var user in users)
        {
            // 1–2 addresses
            var addresses = addressFaker.Generate(new Faker().Random.Int(1, 2));
            addresses[0].IsDefault = true;
            addresses[0].UserId = user.Id;
            foreach (var a in addresses.Skip(1)) a.UserId = user.Id;
            context.Addresses.AddRange(addresses);

            // 0–1 payment method (skip admins)
            if (user.Role == UserRole.User && new Faker().Random.Bool(0.7f))
            {
                var pm = pmFaker.Generate();
                pm.UserId    = user.Id;
                pm.IsDefault = true;
                context.PaymentMethods.Add(pm);
            }
        }
    }

    // ── 3. Catalogue ──────────────────────────────────────────────────────

    private static (List<Category> categories, List<Product> products, List<PricingPlan> plans)
        SeedCatalogue(AppDbContext context)
    {
        var categories = new List<Category>();
        var products   = new List<Product>();
        var plans      = new List<PricingPlan>();

        for (int i = 0; i < CategoryNames.Length; i++)
        {
            var slug = CategoryNames[i].ToLower().Replace(" ", "-");
            var cat  = new Category
            {
                Slug         = slug,
                ImageUrl     = $"https://picsum.photos/seed/{slug}/800/400",
                DisplayOrder = i,
            };

            cat.Translations.Add(new CategoryTranslation { Locale = LocaleLang.Fr, Name = CategoryNames[i], Description = new Faker("fr").Lorem.Sentence() });
            cat.Translations.Add(new CategoryTranslation { Locale = LocaleLang.En, Name = CategoryNames[i], Description = new Faker().Lorem.Sentence() });

            categories.Add(cat);
        }
        context.Categories.AddRange(categories);

        var f = new Faker();
        foreach (var cat in categories)
        {
            int count = f.Random.Int(2, 3);
            for (int p = 0; p < count; p++)
            {
                var prefix      = f.PickRandom(ProductPrefixes);
                var suffix      = f.PickRandom(ProductSuffixes);
                var productSlug = $"{prefix}-{suffix}".ToLower().Replace(" ", "-");

                var product = new Product
                {
                    Category       = cat,
                    Slug           = $"{productSlug}-{f.Random.AlphaNumeric(4)}",
                    TechnicalSpecs = $"Platforms: Windows, macOS, Linux | SLA: {f.Random.Int(95, 99)}% uptime | Support: 24/7 | MaxDevices: {f.Random.Int(10, 1000)}",
                    Status         = f.Random.Bool(0.85f) ? ProductStatus.Available : ProductStatus.Unavailable,
                    IsFeatured     = f.Random.Bool(0.2f),
                    CreatedAt      = f.Date.Past(1).ToUniversalTime(),
                    UpdatedAt      = DateTime.UtcNow,
                };

                product.Translations.Add(new ProductTranslation { Locale = LocaleLang.Fr, Name = $"{prefix} {suffix}", Description = new Faker("fr").Lorem.Paragraphs(2) });
                product.Translations.Add(new ProductTranslation { Locale = LocaleLang.En, Name = $"{prefix} {suffix}", Description = new Faker().Lorem.Paragraphs(2) });

                // 3 images
                for (int img = 0; img < 3; img++)
                    product.Images.Add(new ProductImage { ImageUrl = $"https://picsum.photos/seed/{prefix}-{img}/800/600", DisplayOrder = img });

                // ── Pricing plans + tiers ─────────────────────────────────
                // Monthly plan — per-user and per-device tiers
                var monthlyPlan = new PricingPlan
                {
                    Product       = product,
                    Name          = "Mensuel",
                    BillingPeriod = BillingPeriod.Monthly,
                    DiscountPercent = 0,
                };
                monthlyPlan.PricingTiers.Add(new PricingTier { unitType = BillingUnit.User,   minQuantity = 1,   maxQuantity = 10,  PricePerUnit = Math.Round(f.Finance.Amount(5, 20), 2) });
                monthlyPlan.PricingTiers.Add(new PricingTier { unitType = BillingUnit.User,   minQuantity = 11,  maxQuantity = 50,  PricePerUnit = Math.Round(f.Finance.Amount(3, 10), 2) });
                monthlyPlan.PricingTiers.Add(new PricingTier { unitType = BillingUnit.Device, minQuantity = 1,   maxQuantity = 25,  PricePerUnit = Math.Round(f.Finance.Amount(3, 12), 2) });
                monthlyPlan.PricingTiers.Add(new PricingTier { unitType = BillingUnit.Device, minQuantity = 26,  maxQuantity = 100, PricePerUnit = Math.Round(f.Finance.Amount(2,  8), 2) });

                // Yearly plan — ~17% cheaper per unit (2 months free)
                var yearlyPlan = new PricingPlan
                {
                    Product         = product,
                    Name            = "Annuel",
                    BillingPeriod   = BillingPeriod.Yearly,
                    DiscountPercent = 17,
                };
                yearlyPlan.PricingTiers.Add(new PricingTier { unitType = BillingUnit.User,   minQuantity = 1,   maxQuantity = 10,  PricePerUnit = Math.Round(f.Finance.Amount(40, 180), 2) });
                yearlyPlan.PricingTiers.Add(new PricingTier { unitType = BillingUnit.User,   minQuantity = 11,  maxQuantity = 50,  PricePerUnit = Math.Round(f.Finance.Amount(25, 100), 2) });
                yearlyPlan.PricingTiers.Add(new PricingTier { unitType = BillingUnit.Device, minQuantity = 1,   maxQuantity = 25,  PricePerUnit = Math.Round(f.Finance.Amount(25, 110), 2) });
                yearlyPlan.PricingTiers.Add(new PricingTier { unitType = BillingUnit.Device, minQuantity = 26,  maxQuantity = 100, PricePerUnit = Math.Round(f.Finance.Amount(15,  70), 2) });

                plans.Add(monthlyPlan);
                plans.Add(yearlyPlan);
                product.PricingPlans.Add(monthlyPlan);
                product.PricingPlans.Add(yearlyPlan);
                products.Add(product);
            }
        }

        context.Products.AddRange(products);
        return (categories, products, plans);
    }

    // ── 4. CMS ────────────────────────────────────────────────────────────

    private static void SeedCms(AppDbContext context)
    {
        // Carousel
        for (int i = 0; i < 4; i++)
        {
            var seed  = new Faker().Random.AlphaNumeric(6);
            var slide = new CarouselSlide
            {
                ImageUrl     = $"https://picsum.photos/seed/{seed}/1200/500",
                DisplayOrder = i,
                IsActive     = true,
            };
            slide.Translations.Add(new CarouselSlideTranslation { Locale = LocaleLang.Fr, Title = new Faker("fr").Company.CatchPhrase(), Subtitle = new Faker("fr").Lorem.Sentence(), ButtonText = "Découvrir" });
            slide.Translations.Add(new CarouselSlideTranslation { Locale = LocaleLang.En, Title = new Faker().Company.CatchPhrase(),     Subtitle = new Faker().Lorem.Sentence(),     ButtonText = "Discover" });
            context.CarouselSlides.Add(slide);
        }

        // Site settings
        var missionSetting = new SiteSetting { SettingKey = "homepage_mission_text" };
        missionSetting.Translations.Add(new SiteSettingTranslation { Locale = LocaleLang.Fr, Setting = missionSetting, SettingValue = "Cyna protège vos entreprises grâce à des solutions SaaS de cybersécurité de pointe." });
        missionSetting.Translations.Add(new SiteSettingTranslation { Locale = LocaleLang.En, Setting = missionSetting, SettingValue = "Cyna protects your business with cutting-edge SaaS cybersecurity solutions." });
        context.SiteSettings.Add(missionSetting);
    }

    // ── 5. Promo codes ────────────────────────────────────────────────────

    private static List<PromoCode> SeedPromoCodes(AppDbContext context)
    {
        var promos = new List<PromoCode>
        {
            new() { Code = "WELCOME10", DiscountPercent = 10, IsActive = true,  ExpiresAt = DateTime.UtcNow.AddMonths(6) },
            new() { Code = "CYBER20",   DiscountPercent = 20, IsActive = true,  ExpiresAt = DateTime.UtcNow.AddMonths(3) },
            new() { Code = "EXPIRED",   DiscountPercent = 15, IsActive = false, ExpiresAt = DateTime.UtcNow.AddDays(-1)  },
        };
        context.PromoCodes.AddRange(promos);
        return promos;
    }

    // ── 6. Cart items ─────────────────────────────────────────────────────

    private static void SeedCartItems(AppDbContext context, List<User> users, List<Product> products, List<PricingPlan> plans)
    {
        var f        = new Faker();
        var regUsers = users.Where(u => u.Role == UserRole.User).Take(5).ToList();

        foreach (var user in regUsers)
        {
            int itemCount    = f.Random.Int(1, 3);
            var pickedProducts = f.Random.ListItems(products, itemCount);

            foreach (var product in pickedProducts)
            {
                var plan = f.PickRandom(product.PricingPlans.ToList());
                context.CartItems.Add(new CartItem
                {
                    User            = user,
                    Product         = product,
                    PricingPlan     = plan,
                    // QuantityUsers / QuantityDevices replace the old Quantity field
                    QuantityUsers   = f.Random.Int(1, 20),
                    QuantityDevices = f.Random.Int(1, 10),
                });
            }
        }
    }

    // ── 7. Subscriptions → Orders → Items → Invoices ─────────────────────

    private static void SeedOrdersAndSubscriptions(
        AppDbContext context,
        List<User> users,
        List<Product> products,
        List<PricingPlan> plans,
        List<PromoCode> promos)
    {
        var f        = new Faker();
        var regUsers = users.Where(u => u.Role == UserRole.User).ToList();

        foreach (var user in regUsers)
        {
            var address = context.Addresses.Local.FirstOrDefault(a => a.UserId == user.Id)
                       ?? context.Addresses.FirstOrDefault(a => a.UserId == user.Id);
            if (address is null) continue;

            int orderCount = f.Random.Int(1, 3);
            for (int o = 0; o < orderCount; o++)
            {
                var createdAt = f.Date.Past(2).ToUniversalTime();
                var status    = f.PickRandom(OrderStatuses);

                var product = f.PickRandom(products);
                var plan    = f.PickRandom(product.PricingPlans.ToList());

                var periodStart = createdAt;
                var periodEnd   = plan.BillingPeriod == BillingPeriod.Monthly
                    ? periodStart.AddMonths(1)
                    : periodStart.AddYears(1);

                var subscription = new Subscription
                {
                    User                 = user,
                    Product              = product,
                    PricingPlan          = plan,
                    StripeSubscriptionId = $"sub_{f.Random.AlphaNumeric(24)}",
                    Status               = f.PickRandom(SubStatuses),
                    CurrentPeriodStart   = periodStart,
                    CurrentPeriodEnd     = periodEnd,
                    AutoRenew            = f.Random.Bool(0.7f),
                };
                context.Subscriptions.Add(subscription);

                // Build order items
                int itemCount  = f.Random.Int(1, 3);
                var orderItems = new List<OrderItem>();
                for (int i = 0; i < itemCount; i++)
                {
                    var p2  = f.PickRandom(products);
                    var pl2 = f.PickRandom(p2.PricingPlans.ToList());

                    // Quantities per dimension (users / devices)
                    int qtyUsers   = f.Random.Int(1, 20);
                    int qtyDevices = f.Random.Int(1, 10);

                    orderItems.Add(new OrderItem
                    {
                        Product             = p2,
                        PricingPlan         = pl2,
                        ProductNameSnapshot = p2.Translations.FirstOrDefault(t => t.Locale == LocaleLang.Fr)?.Name ?? p2.Slug,
                        PlanNameSnapshot    = pl2.Name,
                        QuantityUsers       = qtyUsers,
                        QuantityDevices     = qtyDevices,
                    });
                }

                // Compute total from PricingTiers: pick the cheapest matching tier per item dimension
                decimal total = orderItems.Sum(item =>
                {
                    var tiers        = item.PricingPlan.PricingTiers.ToList();
                    decimal userCost = ResolveLineCost(tiers, BillingUnit.User,   item.QuantityUsers);
                    decimal devCost  = ResolveLineCost(tiers, BillingUnit.Device, item.QuantityDevices);
                    return userCost + devCost;
                });

                var order = new Order
                {
                    User                  = user,
                    Subscription          = subscription,
                    BillingAddress        = address,
                    Status                = status,
                    TotalAmount           = Math.Round(total, 2),
                    StripePaymentIntentId = $"pi_{f.Random.AlphaNumeric(24)}",
                    CreatedAt             = createdAt,
                };

                foreach (var item in orderItems)
                {
                    item.Order = order;
                    order.Items.Add(item);
                }

                // Apply promo occasionally
                if (f.Random.Bool(0.25f) && status == OrderStatus.Paid)
                {
                    var promo    = f.PickRandom(promos.Where(p => p.IsActive).ToList());
                    var discount = Math.Round(total * promo.DiscountPercent / 100m, 2);
                    order.PromoCodes.Add(new OrderPromoCode
                    {
                        Order                 = order,
                        PromoCode             = promo,
                        AppliedDiscountAmount = discount,
                    });
                }

                // Invoice for paid orders
                if (status == OrderStatus.Paid)
                {
                    order.Invoices.Add(new Invoice
                    {
                        Order         = order,
                        InvoiceNumber = $"CYNA-{createdAt.Year}-{f.Random.Int(1000, 9999)}",
                        PdfUrl        = $"https://storage.cyna.fr/invoices/{f.Random.AlphaNumeric(16)}.pdf",
                        IssuedAt      = createdAt.AddMinutes(f.Random.Int(1, 60)),
                    });
                }

                context.Orders.Add(order);
            }
        }
    }

    /// <summary>
    /// Finds the best-matching tier for a given unit type + quantity and returns the line cost.
    /// Falls back to the first available tier of that unit type if no range matches exactly.
    /// Returns 0 if no tier exists for that unit type.
    /// </summary>
    private static decimal ResolveLineCost(List<PricingTier> tiers, BillingUnit unit, int qty)
    {
        if (qty <= 0) return 0m;

        var unitTiers = tiers.Where(t => t.unitType == unit).ToList();
        if (unitTiers.Count == 0) return 0m;

        // Best match: a tier whose range contains qty
        var match = unitTiers.FirstOrDefault(t => qty >= t.minQuantity && qty <= t.maxQuantity)
                 // Fallback: the tier with the highest minQuantity that is still ≤ qty (volume overflow)
                 ?? unitTiers.Where(t => t.minQuantity <= qty).MaxBy(t => t.minQuantity)
                 // Last resort: cheapest tier
                 ?? unitTiers.MinBy(t => t.PricePerUnit)!;

        return Math.Round(match.PricePerUnit * qty, 2);
    }

    // ── 8. Support ────────────────────────────────────────────────────────

    private static void SeedSupport(AppDbContext context, List<User> users)
    {
        var f              = new Faker("fr");
        ContactStatus[] contactStatuses = [ContactStatus.InProgress, ContactStatus.Closed, ContactStatus.New, ContactStatus.Resolved];

        // Contact messages — some from guests, some from registered users
        for (int i = 0; i < 10; i++)
        {
            var linkedUser = f.Random.Bool(0.6f) ? f.PickRandom(users) : null;
            context.ContactMessages.Add(new ContactMessage
            {
                UserId    = linkedUser?.Id,
                Email     = linkedUser?.Email ?? f.Internet.Email(),
                Subject   = f.Lorem.Sentence(4),
                Message   = f.Lorem.Paragraphs(1),
                Status    = f.PickRandom(contactStatuses),
                CreatedAt = f.Date.Past(1).ToUniversalTime(),
            });
        }

        // Chatbot conversations
        foreach (var user in users.Take(5))
        {
            var conv = new ChatbotConversation
            {
                UserId           = user.Id,
                EscalatedToHuman = f.Random.Bool(0.2f),
                StartedAt        = f.Date.Past(1).ToUniversalTime(),
            };

            int msgCount = f.Random.Int(3, 8);
            for (int m = 0; m < msgCount; m++)
            {
                conv.Messages.Add(new ChatbotMessage
                {
                    Sender    = m % 2 == 0 ? ChatbotSender.User : ChatbotSender.Bot,
                    Content   = f.Lorem.Sentence(),
                    CreatedAt = conv.StartedAt.AddMinutes(m * 2),
                });
            }

            context.ChatbotConversations.Add(conv);
        }
    }
}