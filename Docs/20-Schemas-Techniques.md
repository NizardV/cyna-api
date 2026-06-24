# Schémas Techniques — Cyna API

## 🎯 Objectif du document

Centraliser **tous les schémas techniques** du projet Cyna API en un seul endroit, dans un ordre
qui va du plus large (contexte / déploiement réseau) au plus fin (classes, modèle de données).
Ce document sert de **référence visuelle** : chaque schéma est accompagné d'une légende et d'un
renvoi vers le document fonctionnel qui en détaille la logique.

> Tous les diagrammes sont écrits en **Mermaid** (rendu natif sur Azure DevOps Repos et GitHub).
> Ils sont donc versionnés *as code* : toute évolution du système doit s'accompagner d'une mise à
> jour du diagramme correspondant dans ce fichier.

### Index des schémas

| # | Schéma | Type | Couvre |
|---|---|---|---|
| 1 | Diagramme de contexte | C4 niveau 1 | Acteurs et systèmes externes |
| 2 | Diagramme de déploiement / réseau | Déploiement | Infrastructure, conteneurs, flux réseau |
| 3 | Diagramme de composants (couches) | C4 niveau 2/3 | Projets `.csproj`, dépendances |
| 4 | Diagramme de classes UML (domaine) | UML classes | Entités métier et relations |
| 5 | Modèle physique de données (ERD) | Entité-association | Tables, clés, cardinalités |
| 6 | Index des diagrammes de séquence | UML séquence | Renvois vers les flux détaillés |
| 7 | Diagrammes d'état | UML état | Cycles de vie (commande, abonnement, 2FA) |

---

## 1. 🌍 Diagramme de contexte (C4 — niveau 1)

Vue la plus haute : qui utilise le système, et de quels systèmes externes il dépend.

```mermaid
flowchart TB
    subgraph Acteurs
        VISITOR([Visiteur anonyme])
        CLIENT([Client authentifié])
        ADMIN([Administrateur / Super Admin])
    end

    subgraph Cyna["Système Cyna"]
        WEB[Front Web<br/>React / Vite<br/>repo Cyna-Web]
        APP[App mobile<br/>repo Cyna-App]
        API[(Cyna API<br/>ASP.NET Core / .NET 10)]
    end

    subgraph Externes["Systèmes externes"]
        STRIPE[[Stripe<br/>paiement & abonnements]]
        RESEND[[Resend<br/>emails transactionnels]]
        PG[(PostgreSQL)]
    end

    VISITOR --> WEB
    CLIENT --> WEB
    CLIENT --> APP
    ADMIN --> WEB

    WEB -->|HTTPS / cookies JWT| API
    APP -->|HTTPS / cookies JWT| API

    API -->|SDK Stripe.net + webhook| STRIPE
    API -->|HTTP API| RESEND
    API -->|EF Core / Npgsql| PG
    STRIPE -.->|webhook signé invoice.paid| API
```

**Légende** : Cyna API est le **back-end unique** consommé par deux front-ends (web et mobile).
Trois dépendances externes : Stripe (paiement), Resend (email), PostgreSQL (persistance).
Détails dans [`00-Architecture-Generale.md`](00-Architecture-Generale.md).

---

## 2. 🖧 Diagramme de déploiement / réseau

Vue d'exécution : conteneurs, ports, reverse proxy et flux réseau, **reconstituée à partir de
[`docker-compose.yml`](../docker-compose.yml), du pipeline [`cd-cloud-api.yml`](../cd-cloud-api.yml)
et de la configuration CORS de [`Program.cs`](../Api/Program.cs)**.

> ℹ️ Le détail complet de l'infrastructure (Terraform/scripts, reverse proxy, pare-feu, sauvegardes)
> est porté par le dépôt **Cyna-Infra** et sera documenté séparément. Le schéma ci-dessous décrit
> la **vue déploiement côté API**, suffisante pour comprendre comment l'API est exposée et reliée
> à ses dépendances.

```mermaid
flowchart LR
    subgraph Internet
        U([Navigateur / App])
    end

    subgraph OVH["Serveur OVH (Docker host)"]
        direction TB
        NGINX[Reverse proxy<br/>HTTPS / TLS]

        subgraph stagingNet["Réseau Docker — staging (projet cyna-staging)"]
            APISTG[Conteneur api:staging<br/>127.0.0.1:4001 vers 8080]
            DBSTG[(postgres:16-alpine<br/>cyna-db staging)]
            APISTG --> DBSTG
        end

        subgraph prodNet["Réseau Docker — prod (projet cyna-prod)"]
            APIPRD[Conteneur api:prod<br/>127.0.0.1:4000 vers 8080]
            DBPRD[(postgres:16-alpine<br/>cyna-db prod)]
            APIPRD --> DBPRD
        end
    end

    subgraph SaaS["Services managés"]
        STRIPE[[Stripe]]
        RESEND[[Resend]]
    end

    GHCR[(GHCR<br/>ghcr.io/nizardv/cyna-api)]

    U -->|443 HTTPS| NGINX
    NGINX -->|projet-cyna.fr| APIPRD
    NGINX -->|staging.projet-cyna.fr| APISTG

    APIPRD -->|HTTPS| STRIPE
    APIPRD -->|HTTPS| RESEND
    STRIPE -.->|webhook /payments/webhook| NGINX

    GHCR -.->|docker pull image taguée| APIPRD
    GHCR -.->|docker pull image taguée| APISTG
```

### Points clés du déploiement

| Élément | Valeur | Source |
|---|---|---|
| Image runtime | `mcr.microsoft.com/dotnet/aspnet` (.NET 10), build multi-stage | [`Dockerfile`](../Dockerfile) |
| Utilisateur conteneur | **non-root** | `Dockerfile` |
| Port interne API | `8080` (`ASPNETCORE_URLS=http://+:8080`) | `docker-compose.yml` |
| Port exposé prod | `127.0.0.1:4000` (derrière reverse proxy) | `cd-cloud-api.yml` |
| Port exposé staging | `127.0.0.1:4001` | `cd-cloud-api.yml` |
| Base de données | `postgres:16-alpine`, volume `postgres_data` | `docker-compose.yml` |
| Exposition BDD | `127.0.0.1:5432` en local uniquement (jamais publique) | `docker-compose.yml` |
| Registre d'images | GitHub Container Registry (GHCR) | `ci-api.yml` |
| Health check | `GET /health` (validé après chaque déploiement) | `cd-cloud-api.yml` |

> 🔒 La base PostgreSQL n'est **jamais exposée publiquement** : le mapping de port est lié à
> `127.0.0.1`, et l'API la joint par le réseau Docker interne (`Host=db`). Voir
> [`40-Securite-et-Conformite.md`](40-Securite-et-Conformite.md).

### Origines CORS autorisées par environnement

| Environnement | Origines | Détail |
|---|---|---|
| Development | `http://localhost:5173`, `https://localhost:5173` | Front Vite local |
| Staging | `https://staging.projet-cyna.fr` | |
| Production | `https://projet-cyna.fr`, `https://www.projet-cyna.fr` | |

`AllowCredentials()` est activé (transmission des cookies JWT cross-origin).

---

## 3. 🧱 Diagramme de composants (architecture en couches)

Vue interne du back-end : les cinq projets `.csproj` et leur règle de dépendance
(Clean / Onion Architecture). Détail dans [`00-Architecture-Generale.md`](00-Architecture-Generale.md).

```mermaid
flowchart TD
    subgraph Présentation
        API["Api<br/>Controllers · Program.cs<br/>Extensions · Interceptors"]
    end
    subgraph Métier
        APP["Application<br/>Services + Interfaces de service"]
    end
    subgraph Coeur
        DOM["Domain<br/>Entities · DTOs"]
    end
    subgraph Accès données & externe
        INFRA["Infrastructure<br/>AppDbContext · Repositories EF<br/>Security JWT · Payments Stripe"]
    end
    TOOLS["Tools<br/>Enums · Helpers (hash, OTP, TOTP, email, claims)"]

    API --> APP
    API --> INFRA
    APP --> DOM
    APP --> INFRA
    INFRA --> DOM
    DOM --> TOOLS
    API --> TOOLS
    APP --> TOOLS
    INFRA --> TOOLS

    classDef core fill:#e8f0fe,stroke:#4285f4;
    class DOM core;
```

**Règle de dépendance** : tout pointe *vers l'intérieur* (`Domain`). `Application` dépend des
**interfaces** de `Infrastructure` (`IUserRepository`, `IPaymentService`…), jamais des
implémentations concrètes — ce qui permet de tester le métier sans base de données et de changer
de fournisseur (SQLite ↔ PostgreSQL, Mock ↔ Stripe).

---

## 4. 🎭 Diagramme de classes UML (domaine)

Vue UML des **principales entités métier** et de leurs associations. Les entités de traduction,
CMS et chatbot sont omises ici pour la lisibilité (présentes dans l'ERD §5). Source :
`Domain/Entities/*`, détail dans [`08-Base-de-donnees.md`](08-Base-de-donnees.md).

```mermaid
classDiagram
    class User {
        +int Id
        +string Email
        +string PasswordHash
        +string FirstName
        +string LastName
        +UserRole Role
        +bool IsEmailVerified
        +bool IsDisabled
        +string TwoFactorSecret
        +bool TwoFactorEnabled
        +string StripeCustomerId
        +string RefreshToken
        +DateTime RefreshTokenExpiryTime
        +DateTime CreatedAt
    }
    class Company {
        +int Id
        +string Name
    }
    class Address {
        +int Id
        +string Line1
        +string City
        +string PostalCode
        +string Country
    }
    class PaymentMethod {
        +int Id
        +string StripePaymentMethodId
        +CardBrand Brand
    }
    class CartItem {
        +int Id
        +int QuantityUsers
        +int QuantityDevices
    }
    class Order {
        +int Id
        +OrderStatus Status
        +decimal Subtotal
        +decimal TaxAmount
        +decimal Total
        +string StripePaymentIntentId
        +DateTime CreatedAt
    }
    class OrderItem {
        +int Id
        +string ProductNameSnapshot
        +string PlanNameSnapshot
        +int QuantityUsers
        +int QuantityDevices
        +decimal UnitPriceUsers
        +decimal UnitPriceDevices
    }
    class Invoice {
        +int Id
        +string InvoiceNumber
        +string PdfUrl
    }
    class Subscription {
        +int Id
        +SubscriptionStatus Status
        +DateTime CurrentPeriodStart
        +DateTime CurrentPeriodEnd
        +bool AutoRenew
        +string StripeSubscriptionId
    }
    class Category {
        +int Id
        +string Slug
        +int DisplayOrder
    }
    class Product {
        +int Id
        +string Slug
        +ProductStatus Status
        +bool IsFeatured
        +int DisplayOrder
        +string TechnicalSpecs
    }
    class PricingPlan {
        +int Id
        +BillingPeriod BillingPeriod
        +int DiscountPercent
        +int MaxUsersCheckout
        +int MaxDevicesCheckout
    }
    class PricingTier {
        +int Id
        +BillingUnit unitType
        +int minQuantity
        +int maxQuantity
        +decimal PricePerUnit
    }

    Company "0..1" --> "0..*" User : emploie
    User "1" --> "0..*" Address
    User "1" --> "0..*" PaymentMethod
    User "1" --> "0..*" CartItem
    User "1" --> "0..*" Order
    User "1" --> "0..*" Subscription
    Order "1" --> "1..*" OrderItem
    Order "1" --> "0..*" Invoice
    Order "0..1" --> "0..*" Subscription
    OrderItem "0..*" --> "1" Product
    OrderItem "0..*" --> "1" PricingPlan
    Subscription "0..*" --> "1" Product
    Subscription "0..*" --> "1" PricingPlan
    Category "1" --> "0..*" Product
    Product "1" --> "1..*" PricingPlan
    PricingPlan "1" --> "1..*" PricingTier
    CartItem "0..*" --> "1" PricingPlan
```

> 🔒 Les associations `OrderItem → Product/PricingPlan` et `Subscription → Product/PricingPlan` sont
> configurées en `DeleteBehavior.Restrict` : un produit ou un plan référencé par l'historique
> commercial **ne peut pas être supprimé** (voir [`08-Base-de-donnees.md`](08-Base-de-donnees.md)).

---

## 5. 🗄️ Modèle physique de données (ERD complet)

Vue exhaustive des tables, **y compris** les entités de traduction (i18n), CMS et chatbot.

```mermaid
erDiagram
    Company ||--o{ User : ""
    User ||--o{ CartItem : ""
    User ||--o{ Order : ""
    User ||--o{ Subscription : ""
    User ||--o{ Address : ""
    User ||--o{ PaymentMethod : ""
    User ||--o{ EmailVerificationCode : ""
    User ||--o{ PasswordResetCode : ""
    User ||--o{ ContactMessage : ""
    User ||--o{ ChatbotConversation : ""

    Order ||--|{ OrderItem : ""
    Order ||--o{ Invoice : ""
    Order ||--o{ OrderPromoCode : ""
    OrderItem }o--|| Product : ""
    OrderItem }o--|| PricingPlan : ""
    OrderPromoCode }o--|| PromoCode : ""

    Subscription }o--|| Product : ""
    Subscription }o--|| PricingPlan : ""

    Category ||--o{ Product : ""
    Category ||--o{ CategoryTranslation : ""
    Product ||--o{ ProductTranslation : ""
    Product ||--o{ ProductImage : ""
    Product ||--|{ PricingPlan : ""
    PricingPlan ||--|{ PricingTier : ""

    CarouselSlide ||--o{ CarouselSlideTranslation : ""
    SiteSetting ||--o{ SiteSettingTranslation : ""
    ChatbotConversation ||--o{ ChatbotMessage : ""
```

### Index uniques (rappel)

| Type | Entités concernées |
|---|---|
| Simple | `User.Email`, `Product.Slug`, `Category.Slug`, `Invoice.InvoiceNumber`, `PromoCode.Code`, `Subscription.StripeSubscriptionId`, `PaymentMethod.StripePaymentMethodId`, `SiteSetting.SettingKey` |
| Composite (unicité par locale) | `(CategoryId, Locale)`, `(ProductId, Locale)`, `(SlideId, Locale)`, `(SettingId, Locale)` |

Détail complet : [`08-Base-de-donnees.md`](08-Base-de-donnees.md).

---

## 6. 🔁 Index des diagrammes de séquence

Les diagrammes de séquence détaillés vivent dans les documents fonctionnels concernés (au plus
près de la logique qu'ils décrivent). Récapitulatif :

| Flux | Document |
|---|---|
| Connexion standard (login utilisateur) | [`01-Authentification-JWT-2FA.md`](01-Authentification-JWT-2FA.md) §3 |
| Connexion admin avec 2FA (bootstrap-aware) | [`01-Authentification-JWT-2FA.md`](01-Authentification-JWT-2FA.md) §4 |
| Vérification d'email (OTP) | [`02-Email-OTP.md`](02-Email-OTP.md) §3 |
| Réinitialisation de mot de passe (OTP) | [`02-Email-OTP.md`](02-Email-OTP.md) §4 |
| Paiement par abonnement Stripe (init → webhook) | [`PAIEMENT-STRIPE.md`](PAIEMENT-STRIPE.md) |
| Création de commande | [`05-Panier-Commandes.md`](05-Panier-Commandes.md) §2 |
| Agrégation page d'accueil (BFF) | [`09-CMS-PageAccueil.md`](09-CMS-PageAccueil.md) §1 |

---

## 7. 🔄 Diagrammes d'état (cycles de vie)

### Commande (`Order.Status`)

```mermaid
stateDiagram-v2
    direction LR
    [*] --> Pending: POST /payments/subscription
    Pending --> Paid: webhook invoice.paid / payment_intent.succeeded
    Pending --> Failed: webhook invoice.payment_failed
    Paid --> Refunded: remboursement
    Pending --> Cancelled: annulation
    Paid --> [*]
```

### Abonnement (`Subscription.Status`)

```mermaid
stateDiagram-v2
    direction LR
    [*] --> Pending: création (paiement en attente)
    Pending --> Active: webhook paiement confirmé
    Active --> Suspended: échec de renouvellement
    Active --> Cancelled: annulation client
    Active --> Expired: fin de période sans renouvellement
    Cancelled --> [*]
    Expired --> [*]
```

### 2FA admin (cycle bootstrap)

```mermaid
stateDiagram-v2
    [*] --> SansSecret: création du compte admin
    SansSecret --> SecretEnAttente: POST /auth/2fa/setup
    SecretEnAttente --> SecretEnAttente: POST /auth/2fa/setup (re-scan)
    SecretEnAttente --> Actif: POST /auth/2fa/confirm (code valide)
    Actif --> Actif: connexions suivantes exigent un TOTP
```

> Détail des transitions et de la logique « bootstrap » : [`01-Authentification-JWT-2FA.md`](01-Authentification-JWT-2FA.md).

---

## 🔗 Documents liés

* [`00-Architecture-Generale.md`](00-Architecture-Generale.md) — description textuelle de l'architecture
* [`08-Base-de-donnees.md`](08-Base-de-donnees.md) — modèle de données détaillé
* [`50-Scalabilite-et-Performance.md`](50-Scalabilite-et-Performance.md) — vue performance du déploiement
