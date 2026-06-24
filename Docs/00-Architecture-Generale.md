# Architecture Générale — Cyna API

## 🎯 Objectif du document

Donner une vue d'ensemble de l'architecture technique du projet **Cyna API** : organisation en couches, injection de dépendances, pipeline HTTP, configuration de la base de données et outillage de build/déploiement. Ce document est le point d'entrée pour comprendre comment les autres documents (`01-*`, `02-*`, …) s'articulent entre eux.

---

## 🏗️ 1. Style architectural

Le projet suit une **architecture en couches inspirée de la Clean Architecture / Onion Architecture**, avec une séparation stricte des responsabilités par projet `.csproj` :

```
┌─────────────────────────────────────────────────────────┐
│                          Api                            │
│   Controllers · Program.cs · Extensions · Interceptors  │
└───────────────────────────┬─────────────────────────────┘
                            │ dépend de
┌───────────────────────────▼───────────────────────────────┐
│                       Application                         │
│        Services (logique métier) · Interfaces             │
└───────────────────────────┬───────────────────────────────┘
                            │ dépend de
┌───────────────────────────▼───────────────────────────────┐
│                          Domain                           │
│   Entities · Dto · Enums (Tools) · Aucune dépendance      │
└───────────────────────────▲───────────────────────────────┘
                            │ implémente les interfaces de
┌───────────────────────────┴───────────────────────────────┐
│                      Infrastructure                       │
│  Repositories (EF Core) · AppDbContext · Security (JWT)   │
└───────────────────────────────────────────────────────────┘
```

### Règle de dépendance

* **`Domain`** ne dépend de rien (sauf `Tools` pour les enums partagés) : c'est le cœur métier (entités EF, DTOs).
* **`Application`** définit des **interfaces** (`IUserService`, `IOrderService`, …) et leurs implémentations (`UserService`, `OrderService`, …). Elle dépend de `Domain` et `Infrastructure.Interfaces` (pas de l'implémentation concrète des repositories).
* **`Infrastructure`** implémente les interfaces de dépôt (`IUserRepository` → `UserRepository`) avec Entity Framework Core, et porte la sécurité bas niveau (génération JWT).
* **`Api`** est la couche de présentation : contrôleurs REST, configuration du pipeline ASP.NET Core, injection de dépendances.
* **`Tools`** est une bibliothèque transverse sans dépendance métier : enums partagés (`UserRole`, `LocaleLang`, …), helpers (hash de mot de passe, OTP, TOTP, emails, claims JWT).

Ce découpage permet de **tester la logique métier (`Application`) indépendamment de la base de données**, et de changer de fournisseur de BDD (SQLite ↔ PostgreSQL) sans toucher au code métier.

---

## 📦 2. Détail des projets

| Projet | Rôle | Dépend de |
|---|---|---|
| `Domain` | Entités EF Core (`Domain/Entities`) et DTOs (`Domain/Dto`) | `Tools` |
| `Application` | Services métier + interfaces de service | `Domain`, `Infrastructure.Interfaces`, `Tools` |
| `Infrastructure` | `AppDbContext`, Repositories EF Core, sécurité JWT | `Domain`, `Tools` |
| `Api` | Contrôleurs, `Program.cs`, configuration, intercepteurs EF | `Application`, `Infrastructure`, `Tools` |
| `Tools` | Enums, helpers transverses (hash, OTP, TOTP, email, claims) | — |
| `UnitTests` / `Api.IntegrationTests` | Tests | tous |

---

## 🔌 3. Injection de dépendances (`AppServicesExtensions`)

Toute l'enregistrement des services se fait dans `Api/Extensions/AppServicesExtensions.cs`, appelé depuis `Program.cs` via `builder.Services.AddAppServices(config)`.

### Repositories (`AddScoped`)

| Interface | Implémentation |
|---|---|
| `IUserRepository` | `UserRepository` |
| `IOrderRepository` | `OrderRepository` |
| `ISubscriptionRepository` | `SubscriptionRepository` |
| `ICatalogRepository` | `CatalogRepository` |
| `ICartRepository` | `CartRepository` |
| `ICarouselRepository` | `CarouselRepository` |
| `ISiteSettingRepository` | `SiteSettingRepository` |
| `ICategoryRepository` | `CategoryRepository` |
| `IProductRepository` | `ProductRepository` |
| `ISearchRepository` | `SearchRepository` |
| `IDashboardRepository` | `DashboardRepository` |

### Services métier (`AddScoped`)

| Interface | Implémentation | Détails |
|---|---|---|
| `IOrderService` | `OrderService` | voir `05-Panier-Commandes.md` |
| `ISubscriptionService` | `SubscriptionService` | |
| `ICatalogService` | `CatalogService` | voir `04-Catalogue-Recherche.md` |
| `IAuthService` | `AuthService` | voir `01-Authentification-JWT-2FA.md` — **enregistré deux fois** : une fois en tant que classe concrète (`AuthService`) pour exposer `SendEmailVerificationOtpInternalAsync` à `UserService`, une fois via l'interface `IAuthService` pour le contrôleur |
| `IUserService` | `UserService` | voir `03-Gestion-Utilisateurs.md` |
| `ICartService` | `CartService` | |
| `ICategoryService` | `CategoryService` | voir `06-Categories.md` |
| `ICmsService` | `CmsService` | voir `09-CMS-PageAccueil.md` |
| `IProductService` | `ProductService` | voir `Docs/ProductAdmin-CRUD.md` |
| `ISearchService` | `SearchService` | |
| `IDashboardService` | `DashboardService` | voir `07-Dashboard-Statistiques.md` |

> ⚠️ **Particularité `AuthService`** : `UserController`/`UserService` ont besoin d'appeler `SendEmailVerificationOtpInternalAsync`, une méthode **publique mais absente de `IAuthService`**. Le code injecte donc la classe concrète `AuthService` en plus de l'interface. C'est un compromis pragmatique, à surveiller si l'interface est testée par mock (`Moq`) — il faudra alors mocker la classe concrète ou refactoriser la méthode dans l'interface.

### Email (Resend)

```csharp
services.AddOptions();
services.AddHttpClient<ResendClient>();
services.Configure<ResendClientOptions>(o => o.ApiToken = config["Resend:ApiKey"]!);
services.AddTransient<IResend, ResendClient>();
services.AddTransient<EmailHelper>();
```

Voir `02-Email-OTP.md` pour le détail des flux OTP/email.

---

## 🚦 4. Pipeline HTTP (`Program.cs`)

L'ordre des middlewares est **critique** et respecté ainsi :

```
1. UseHttpsRedirection()   → force HTTPS
2. UseCors("Frontend")     → CORS avant tout traitement de requête authentifiée
3. UseCookiePolicy()       → gestion des cookies (SameSite, consentement désactivé)
4. UseAuthentication()     → résout l'identité depuis le cookie JWT
5. UseAuthorization()      → applique les [Authorize] / policies
6. MapControllers()        → routage vers les contrôleurs
```

### CORS par environnement

| Environnement | Origines autorisées |
|---|---|
| Development | `http://localhost:5173`, `https://localhost:5173` |
| Staging | `https://staging.projet-cyna.fr` |
| Production | `https://projet-cyna.fr`, `https://www.projet-cyna.fr` |

`AllowCredentials()` est activé partout (nécessaire pour transmettre les cookies JWT cross-origin entre le frontend Vite/React et l'API).

### Documentation API

Configurable via `ApiDocs` (config) : `Scalar` (par défaut) ou `Swagger`. Désactivée en production (`!app.Environment.IsProduction()`). `CustomSchemaIds(type => type.FullName)` est utilisé pour éviter les collisions de noms entre DTOs homonymes dans des namespaces différents (ex. `Home.CategoryDto` vs `Catalog.CategoryDto`).

### Migrations & seed automatiques

Au démarrage, `context.Database.MigrateAsync()` est exécuté systématiquement. Le seed (`DbInitializer.SeedAsync`) ne s'exécute que si l'argument `--seed` est passé **et** que l'environnement n'est pas `Production`.

### Health check

`/health` est exposé via `AddHealthChecks()` / `MapHealthChecks("/health")` — utilisé par le pipeline CD (`cd-cloud-api.yml`) pour valider le déploiement.

---

## 🗄️ 5. Base de données (`DatabaseExtensions`)

Voir le détail complet dans `08-Base-de-donnees.md`. Résumé :

* Provider piloté par la clé de config `DatabaseProvider` (`sqlite` par défaut, `postgres` en option).
* Un intercepteur EF Core (`EfSlowQueryInterceptor`) journalise toute commande SQL dépassant un seuil configurable (`EfPerformanceOptions.SeuilMs`, 200 ms par défaut).
* `RelationalEventId.PendingModelChangesWarning` est ignoré (warning bénin lié aux migrations en développement).

---

## 🐳 6. Build & déploiement

* **Dockerfile** : build multi-stage (.NET 10 SDK → ASP.NET runtime), utilisateur non-root, écoute sur le port `8080`.
* **CI** (`ci-api.yml`) : restore/build/test (.NET 10), analyse SonarCloud, build & push de l'image Docker vers GHCR (tags `staging`/`prod`, `sha-*`, `build-*`).
* **CD** (`cd-cloud-api.yml`) : versioning sémantique automatique sur `main` (analyse des messages de commit conventionnels `feat:`/`fix:`/`feat!:`), déploiement SSH via `docker compose` sur l'infrastructure OVH, health check post-déploiement.
* **docker-compose.yml** : PostgreSQL + API, variables d'environnement pour la chaîne de connexion et le secret JWT.

---

## 🔗 Documents liés

* `01-Authentification-JWT-2FA.md`
* `02-Email-OTP.md`
* `08-Base-de-donnees.md`
* `INSTALLATION.md` *(à la racine du repo)* — procédure d'installation locale