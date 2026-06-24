# Base de Données & Entity Framework Core — Cyna API

## 🎯 Objectif du document

Décrire le modèle de données (entités, relations, index), la configuration multi-fournisseur (SQLite/PostgreSQL), et l'intercepteur de détection des requêtes lentes.

---

## 🗃️ 1. Fournisseur de base de données (`DatabaseExtensions.AddDatabase`)

```csharp
var provider = config["DatabaseProvider"] ?? "sqlite";

services.AddDbContext<AppDbContext>((sp, options) =>
{
    options.AddInterceptors(sp.GetRequiredService<EfSlowQueryInterceptor>());

    if (provider.Equals("postgres", StringComparison.OrdinalIgnoreCase))
        options.UseNpgsql(config.GetConnectionString("DefaultConnection"));
    else
        options.UseSqlite(config.GetConnectionString("DefaultConnection"));

    options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
});
```

* **SQLite par défaut** (zéro configuration en développement local, fichier `CynaApi.db`).
* **PostgreSQL en option**, recommandé pour les tests "en conditions réelles" et **obligatoire comme provider de référence pour générer les migrations** (voir `INSTALLATION.md` à la racine du repo) — SQLite étant trop permissif sur les types (boolean, timestamp), une migration générée sous SQLite pourrait masquer des incompatibilités PostgreSQL.
* L'avertissement `PendingModelChangesWarning` est volontairement ignoré (bruit en développement lorsque le modèle évolue plus vite que les migrations).

---

## 🧩 2. Vue d'ensemble des entités (`AppDbContext`)

```
Company ──< User ──< CartItem
                ├──< Order ──< OrderItem ──> Product / PricingPlan
                │       ├──< Invoice
                │       └──< OrderPromoCode ──> PromoCode
                ├──< Subscription ──> Product / PricingPlan
                ├──< Address
                ├──< PaymentMethod
                ├──< EmailVerificationCode
                ├──< PasswordResetCode
                ├──< ContactMessage
                └──< ChatbotConversation ──< ChatbotMessage

Category ──< Product ──< ProductTranslation
                     ├──< ProductImage
                     └──< PricingPlan ──< PricingTier
         └──< CategoryTranslation

CarouselSlide ──< CarouselSlideTranslation
SiteSetting   ──< SiteSettingTranslation
```

### Regroupement par domaine (dossiers `Domain/Entities/*`)

| Dossier | Entités |
|---|---|
| `AddressAndPayment` | `Address`, `PaymentMethod` |
| `AuthCodes` | `EmailVerificationCode`, `PasswordResetCode` |
| `Catalogue` | `Category`, `CategoryTranslation`, `Product`, `ProductTranslation`, `ProductImage`, `PricingPlan`, `PricingTier` |
| `OrdersAndSubscriptions` | `CartItem`, `Order`, `OrderItem`, `Invoice`, `Subscription` |
| `PromoAndCms` | `CarouselSlide`, `CarouselSlideTranslation`, `SiteSetting`, `SiteSettingTranslation`, `PromoCode`, `OrderPromoCode`, `ContactMessage`, `ChatbotConversation`, `ChatbotMessage` |
| *(racine)* | `User`, `Company` |

---

## 🔑 3. Index uniques

### Simples

| Entité | Colonne |
|---|---|
| `User` | `Email` |
| `PaymentMethod` | `StripePaymentMethodId` |
| `Category` | `Slug` |
| `Product` | `Slug` |
| `Subscription` | `StripeSubscriptionId` |
| `Invoice` | `InvoiceNumber` |
| `PromoCode` | `Code` |
| `SiteSetting` | `SettingKey` |

### Composites (unicité par locale)

| Entité | Colonnes |
|---|---|
| `CategoryTranslation` | `(CategoryId, Locale)` |
| `ProductTranslation` | `(ProductId, Locale)` |
| `CarouselSlideTranslation` | `(SlideId, Locale)` |
| `SiteSettingTranslation` | `(SettingId, Locale)` |

→ empêche d'avoir deux traductions `fr` pour la même catégorie/produit/slide/paramètre.

---

## 🛡️ 4. Comportement de suppression (`DeleteBehavior`)

Par défaut, EF Core applique `Cascade` sur les FK requises. Le code **surcharge explicitement** ce comportement à `Restrict` pour protéger l'historique commercial :

```csharp
mb.Entity<OrderItem>().HasOne(oi => oi.Product)...OnDelete(DeleteBehavior.Restrict);
mb.Entity<OrderItem>().HasOne(oi => oi.PricingPlan)...OnDelete(DeleteBehavior.Restrict);
mb.Entity<Subscription>().HasOne(s => s.Product)...OnDelete(DeleteBehavior.Restrict);
mb.Entity<Subscription>().HasOne(s => s.PricingPlan)...OnDelete(DeleteBehavior.Restrict);
```

**Sans cette configuration**, supprimer un produit effacerait silencieusement (en cascade) les lignes de commandes et abonnements associés — perte de données comptables/légales. Avec `Restrict`, toute tentative de suppression d'un produit/plan référencé échoue **au niveau base de données** ; la couche service l'anticipe en vérifiant explicitement via `HasOrderOrSubscriptionReferencesAsync` / `PlanHasOrderOrSubscriptionReferencesAsync` pour renvoyer un `409 Conflict` propre plutôt qu'une exception SQL brute.

La relation `Order → Subscription` (FK `SubscriptionId`) est explicitement optionnelle (`IsRequired(false)`).

---

## 🐢 5. Intercepteur de requêtes lentes (`EfSlowQueryInterceptor`)

### Principe

Hérite de `DbCommandInterceptor` (EF Core) et journalise **uniquement** les commandes SQL dont la durée d'exécution dépasse un seuil configurable :

```csharp
public class EfPerformanceOptions
{
    public int SeuilMs { get; set; } = 200;  // 200 ms par défaut
}
```

Configurable via `appsettings.json` (section liée à `EfPerformanceOptions` — à vérifier/ajouter si absente, le code utilise `IOptions<EfPerformanceOptions>`).

### Couverture

Intercepte les quatre types d'exécution EF Core (sync **et** async) :

| Méthode interceptée | Type de requête |
|---|---|
| `ReaderExecuted(Async)` | `SELECT` (lecture, `DbDataReader`) |
| `NonQueryExecuted(Async)` | `INSERT`/`UPDATE`/`DELETE` |
| `ScalarExecuted(Async)` | Requêtes retournant une valeur scalaire (`COUNT`, etc.) |

### Identification de l'appelant (`TrouverAppelantRepository`)

```csharp
var frames = new StackTrace(skipFrames: 1, fNeedFileInfo: false).GetFrames();
foreach (var frame in frames)
{
    var methode = frame.GetMethod();
    if (methode?.DeclaringType?.Namespace?.StartsWith("Webzine.Repository", ...) == true)
        return $"{methode.DeclaringType.Name}.{methode.Name}";
}
return "inconnu";
```

⚠️ **Incohérence de namespace identifiée** : le filtre recherche un namespace préfixé `"Webzine.Repository"`, alors que les repositories réels du projet sont dans `Infrastructure.Repositories` (cf. `CategoryRepository`, `ProductRepository`, etc.). **Avec le code actuel, l'appelant ne sera jamais identifié et le log affichera systématiquement `"inconnu"`** pour la classe/méthode d'origine — vestige probable d'un projet précédent (nommé "Webzine"). À corriger en remplaçant la constante par `"Infrastructure.Repositories"` pour restaurer la valeur diagnostique de ce log.

### Coût de performance

Le `StackTrace` n'est instancié **que si le seuil est dépassé** — aucun coût sur le chemin nominal (requêtes rapides), seulement sur les requêtes déjà identifiées comme lentes.

---

## 🌱 6. Migrations & seed

* Migrations gérées par projet : `dotnet ef migrations add <Nom> --project Infrastructure --startup-project Api`.
* Appliquées **automatiquement au démarrage** de l'API (`context.Database.MigrateAsync()` dans `Program.cs`), pour tous les environnements.
* Le seed (`DbInitializer.SeedAsync`) ne s'exécute que sur demande explicite (`dotnet run --project Api -- --seed`) **et** hors environnement `Production` — garde-fou contre un seed accidentel de données de démonstration en production.
* Règle stricte de guillemets pour le SQL brut dans les migrations (sensibilité à la casse PostgreSQL), documentée dans `INSTALLATION.md`.


---

## 🔗 Documents liés

* `00-Architecture-Generale.md`
* `05-Panier-Commandes.md`
* `Docs/ProductAdmin-CRUD.md`