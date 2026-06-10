# Documentation — CRUD Admin Produits

Ce document détaille l'architecture, les DTOs et la logique métier du **CRUD produit (Create/Read/Update/Delete)** réservé aux administrateurs, permettant de gérer les produits complet.

---

## 🎯 Objectif

Exposer un ensemble de routes sécurisées pour que les administrateurs puissent :
- **Lister** tous les produits avec filtrage/pagination
- **Créer** un nouveau produit (nom FR/EN, catégorie, plans tarifaires, spécifications)
- **Éditer** un produit existant (changement de plans, de noms, de status)
- **Supprimer** un produit (avec protection contre les références existantes)

---

## 🔐 Authentification & Autorisation

### Authentification
- **Méthode** : JWT stocké en cookie `cyna_token`
- **TokenValidationParameters** : `RoleClaimType = "role"`
- **Lecture** : Automatique via `credentials: "include"` côté front

### Autorisation
**Policy `AdminOnly`** dans `Program.cs` :
```csharp
options.AddPolicy("AdminOnly", policy =>
    policy.RequireRole("Administrateur", "Super Administrateur"));
```

Appliquée sur **toutes les routes** : GET /products, POST, PUT, DELETE.

---

## 📍 Routes

| Verbe | Endpoint | Statut | Remarques |
|-------|----------|--------|-----------|
| `GET` | `/products` | [AdminOnly] | Liste complète des produits (admin) |
| `GET` | `/products/:id` | [Public] | Détails produit publique (page produit) |
| `GET` | `/products/:id/admin` | [AdminOnly] | Détails complet pour formulaire édition (bilingue) |
| `GET` | `/products/:id/similar` | [Public] | Produits similaires (alias: `/products/similar/:id`) |
| `GET` | `/categories` | [Public] | Catégories pour select formulaire |
| `POST` | `/products` | [AdminOnly] | Créer un produit → 201 Created |
| `PUT` | `/products/:id` | [AdminOnly] | Mettre à jour → 200 OK ou 404/409 |
| `DELETE` | `/products/:id` | [AdminOnly] | Supprimer → 204 No Content ou 404/409 |

### Codes HTTP possibles

| Code | Cas |
|------|-----|
| **200** | Lecture réussie, mise à jour réussie |
| **201** | Création réussie |
| **204** | Suppression réussie (No Content) |
| **400** | Validation échouée (nom vide, category inexistante, status invalide) |
| **401** | Non authentifié |
| **403** | Authentifié mais non-admin |
| **404** | Produit/catégorie inexistant |
| **409** | Conflict (produit/plan réferencé par OrderItem ou Subscription) |

---

## 📦 DTOs

### Requête : `ProductUpsertRequestDto` (POST/PUT)

```csharp
public class ProductUpsertRequestDto {
    [Required, MaxLength(200)] 
    public string NameFr { get; set; } = string.Empty;
    
    public string? NameEn { get; set; }
    public string? DescriptionFr { get; set; }
    public string? DescriptionEn { get; set; }
    
    [Required] 
    public string Status { get; set; } = string.Empty; // Available | Unavailable | OutOfStock | Preview
    
    [Range(1, int.MaxValue)] 
    public int CategoryId { get; set; }
    
    public string? ImageUrl { get; set; }
    public bool IsFeatured { get; set; }
    public int? DisplayOrder { get; set; }
    
    public List<string> TechnicalSpecs { get; set; } = [];
    public List<ProductPricingPlanInputDto> PricingPlans { get; set; } = [];
}

public class ProductPricingPlanInputDto {
    public string Name { get; set; } = string.Empty;
    public string BillingPeriod { get; set; } = string.Empty; // monthly | yearly | lifetime
    public int DiscountPercent { get; set; }
    public int MaxUsersCheckout { get; set; } = 999;
    public int MaxDevicesCheckout { get; set; } = 999;
    public List<ProductPricingTierInputDto> PricingTiers { get; set; } = [];
}

public class ProductPricingTierInputDto {
    public string UnitType { get; set; } = string.Empty; // user | device
    public int MinQty { get; set; }
    public int MaxQty { get; set; }
    public decimal UnitPrice { get; set; }
}
```

**Exemple JSON (création) :**
```json
{
  "nameFr": "Cyna EDR Pro",
  "nameEn": "Cyna EDR Pro",
  "descriptionFr": "Protection anti-malware avancée avec détection comportementale.",
  "descriptionEn": "Advanced malware protection with behavioral detection.",
  "status": "Available",
  "categoryId": 1,
  "imageUrl": "https://example.com/edr-pro.jpg",
  "isFeatured": true,
  "displayOrder": 1,
  "technicalSpecs": [
    "Protection multi-terminaux (Windows, macOS, Linux)",
    "Isolation réseau automatique",
    "Support 24/7 inclus"
  ],
  "pricingPlans": [
    {
      "name": "Mensuel",
      "billingPeriod": "monthly",
      "discountPercent": 0,
      "maxUsersCheckout": 999,
      "maxDevicesCheckout": 999,
      "pricingTiers": [
        {
          "unitType": "user",
          "minQty": 1,
          "maxQty": 5,
          "unitPrice": 99.99
        },
        {
          "unitType": "user",
          "minQty": 6,
          "maxQty": 999,
          "unitPrice": 79.99
        }
      ]
    }
  ]
}
```

### Réponse : `ProductAdminDto` (GET /products/:id/admin)

```csharp
public class ProductAdminDto {
    public int Id { get; set; }
    public string Slug { get; set; }
    public string NameFr { get; set; }
    public string NameEn { get; set; }
    public string DescriptionFr { get; set; }
    public string DescriptionEn { get; set; }
    public string Status { get; set; }
    public int CategoryId { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsFeatured { get; set; }
    public int? DisplayOrder { get; set; }
    public IEnumerable<string> TechnicalSpecs { get; set; }
    public IEnumerable<ProductAdminPricingPlanDto> PricingPlans { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ProductAdminPricingPlanDto {
    public int Id { get; set; }
    public string Name { get; set; }
    public string BillingPeriod { get; set; }
    public int DiscountPercent { get; set; }
    public int MaxUsersCheckout { get; set; }
    public int MaxDevicesCheckout { get; set; }
    public IEnumerable<ProductAdminPricingTierDto> PricingTiers { get; set; }
}

public class ProductAdminPricingTierDto {
    public int Id { get; set; }
    public string UnitType { get; set; }
    public int MinQty { get; set; }
    public int MaxQty { get; set; }
    public decimal UnitPrice { get; set; }
}
```

### Réponse : `ProductAdminListItemDto` (GET /products)

Allégée pour les listes (pas d'arbre tarifaire complet) :
```csharp
public class ProductAdminListItemDto {
    public int Id { get; set; }
    public string Slug { get; set; }
    public string NameFr { get; set; }
    public string Status { get; set; }
    public bool IsFeatured { get; set; }
    public int? DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

---

## 🧠 Logique Métier (ProductService)

### 1. Génération du Slug

À la **création uniquement** :
1. Normalisé depuis `NameFr` via Unicode (FormD → strip accents → FormC)
2. Convertit en kebab-case (regex)
3. **Immutable après création** — protège les URLs externes
4. **Unicité garantie** : boucle suffixe `-2`, `-3`… si doublon

```csharp
// Exemple
"Cyna EDR Pro" → "cyna-edr-pro"
"Cyna EDR Pro" (doublon) → "cyna-edr-pro-2"
```

### 2. Spécifications Techniques

Stockées en **colonne JSON unique** (`Product.TechnicalSpecs : string?`)

**Sérialisation** (création/update) : `JsonConvert.SerializeObject(specs)`

**Désérialisation** (lecture) : JSON avec **fallback** pour anciennes données pipe-séparées
```csharp
// Ancien format seed (pipe-separated)
"Platforms: Windows, macOS | SLA: 99.9% | Support: 24/7"

// Nouveau format (JSON)
["Platforms: Windows, macOS", "SLA: 99.9%", "Support: 24/7"]
```

### 3. Traductions

Upsertées par **locale (fr/en)** :
- Si locale absent → création nouvelle `ProductTranslation`
- Si présent → mise à jour des champs `Name`, `Description`

### 4. Plans Tarifaires

**À la création** : ajout simple

**À l'update** :
1. Récupère plans existants par `Product.PricingPlans`
2. Filtre payload par `BillingPeriod` (monthly, yearly, lifetime)
3. Pour chaque plan en payload :
   - Si existe (même `BillingPeriod`) → mise à jour + remplacement tiers
   - Si n'existe pas → création
4. Pour chaque plan existant **non** en payload :
   - Vérifie `HasOrderOrSubscriptionReferencesAsync(planId)`
   - Si aucune référence → suppression
   - Si référencé → erreur `InvalidOperationException` → **409 Conflict**

### 5. Protection des Données : DeleteBehavior.Restrict

Migration `ProductAdminCrud` ajoute `Restrict` sur toutes les FK entre produits/plans et historique :

```csharp
// ProductRepository > OnModelCreating
mb.Entity<OrderItem>()
    .HasOne(oi => oi.Product).WithMany(p => p.OrderItems)
    .HasForeignKey(oi => oi.ProductId)
    .OnDelete(DeleteBehavior.Restrict);

mb.Entity<OrderItem>()
    .HasOne(oi => oi.PricingPlan).WithMany(pp => pp.OrderItems)
    .HasForeignKey(oi => oi.PricingPlanId)
    .OnDelete(DeleteBehavior.Restrict);

// Idem pour Subscription
```

**Suppression de produit** → 409 Conflict si produit est référencé par au moins un OrderItem ou Subscription.

---

## 🔄 Flux d'Exécution

### Création (POST /products)

```
Frontend (POST /products)
    ↓
ProductController.Post()
    ↓
ProductService.CreateProductAsync(dto)
    ├─ ValidateUpsertAsync() → vérifie status, category existance
    ├─ GenerateUniqueSlugAsync() → crée slug unique depuis NameFr
    ├─ Product nouvelle entité
    ├─ UpsertTranslation(fr, nameF, descFr)
    ├─ UpsertTranslation(en, nameEn, descEn)
    ├─ UpsertMainImage() → ProductImage
    ├─ UpsertPricingPlansAsync() → crée tous les plans + tiers
    └─ SaveChangesAsync()
    ↓
201 Created + ProductAdminDto (avec slug généré)
```

### Édition (PUT /products/:id)

```
Frontend (PUT /products/:id)
    ↓
ProductController.Put()
    ↓
ProductService.UpdateProductAsync(id, dto)
    ├─ Récupère produit existant
    ├─ ValidateUpsertAsync()
    ├─ Mise à jour traductions FR/EN
    ├─ Mise à jour image (remplace si fournie)
    ├─ Mise à jour status, isFeatured, displayOrder
    ├─ UpsertPricingPlansAsync() → remplacement intelligent plans
    │  └─ Pour chaque plan supprimé : vérifie HasOrderOrSubscriptionReferencesAsync
    │     ├─ Aucune ref → suppression
    │     └─ Avec ref → erreur → 409
    └─ SaveChangesAsync()
    ↓
200 OK + ProductAdminDto
ou
409 Conflict (plan référencé)
ou
404 Not Found
```

### Suppression (DELETE /products/:id)

```
Frontend (DELETE /products/:id)
    ↓
ProductController.Delete()
    ↓
ProductService.DeleteProductAsync(id)
    ├─ Récupère produit
    ├─ Vérifie HasOrderOrSubscriptionReferencesAsync(productId)
    │  ├─ Aucune → suppression product (cascade FK)
    │  └─ Avec ref → InvalidOperationException
    ├─ SaveChangesAsync()
    └─ _uow.SaveChangesAsync()
    ↓
204 No Content
ou
409 Conflict
ou
404 Not Found
```

---

## 🧪 Tests Intégration

**File** : `Api.IntegrationTests/Products/ProductCrudTests.cs`

**19 tests** couvrant :

| Test | Cas |
|------|-----|
| `GetAll_AdminRoute_Requires401` | Sans auth |
| `GetAll_AdminRoute_Requires403` | Avec rôle non-admin |
| `GetAll_AdminRoute_Returns200` | Admin liste produits |
| `CreateProduct_Success_Returns201` | Création OK + slug unique |
| `CreateProduct_InvalidStatus_Returns400` | Status invalide |
| `CreateProduct_NonexistentCategory_Returns400` | Category inexistante |
| `CreateProduct_EmptyNameFr_Returns400` | NameFr obligatoire |
| `GetAdminDetails_Returns200_WithBothLocales` | Édition : bilingue OK |
| `GetAdminDetails_NotFound_Returns404` | Produit inexistant |
| `UpdateProduct_ReplacesPricingPlans` | Update + plan replacement |
| `UpdateProduct_NotFound_Returns404` | Update produit inexistant |
| `DeleteProduct_Success_Returns204` | Suppression OK |
| `DeleteProduct_NotFound_Returns404` | Suppression inexistant |
| `DeleteProduct_HasOrderReference_Returns409` | Suppression + commande → 409 |
| `GetProductDetails_Public_RenvoieLesSpecsEnTableau` | Specs : array JSON |
| `GetSimilarProducts_Returns200` | Similaires OK |
| `GetCategories_Returns200` | Categories OK |
| + tests slugs uniques, images, etc. |

---

## 🚀 Points Clés d'Implémentation

### Slug immutable
Le slug générée à la création **ne change jamais**. Cela garantit que :
- Les URLs publiques restent valides indéfiniment
- Les références externes (livres blancs, docs externes) ne se cassent pas
- Si l'admin change le nom, le slug ancien persiste

### Plans suppression intelligente
Un plan ne peut être supprimé que s'il n'a **aucune** référence :
- À un `OrderItem` (commande historique)
- À une `Subscription` (abonnement actif)

Si suppression impossible → 409 Conflict, pas de suppression silencieuse.

### Status Format
- **Base de données** : Enum `ProductStatus` (PascalCase : "Available", "Unavailable", etc.)
- **DTO admin** : PascalCase (`"Available"`)
- **DTO public** : minuscule (`"available"`) — pour compatibilité front legacy

### Images unique
Un seul `ImageUrl` en colonne `Product.ImageUrl`, pas de galerie multi-images en admin (futur : `ProductImage.DisplayOrder` éditable en admin).

---

## 📋 Migration EF Core

**File** : `Infrastructure/Migrations/20260610130147_ProductAdminCrud.cs`

Ajoute :
- `Product.DisplayOrder` (int? nullable) — ordre d'affichage sur home
- `PricingPlan.MaxUsersCheckout` (int, défaut 999) — limite checkout
- `PricingPlan.MaxDevicesCheckout` (int, défaut 999) — limite checkout

Modifie FK avec `DeleteBehavior.Restrict` :
- `OrderItem.ProductId`
- `OrderItem.PricingPlanId`
- `Subscription.ProductId`
- `Subscription.PricingPlanId`

**Backfill** (prod) :
```sql
UPDATE PricingPlans SET MaxUsersCheckout = 999, MaxDevicesCheckout = 999;
```

---

## 📌 Limitations & Évolutions

- **Images multiples** : actuellement une seule image principale (`Product.ImageUrl`). Futur : gérer `ProductImage.DisplayOrder` en admin.
- **Catégories CRUD** : admin GET /categories existant, mais POST/PUT/DELETE non implémentés.
- **Transactions** : migration s'exécute sur ≥ 2 tables, pas de transaction explicite (`DbTransaction`).
- **Audit** : pas de log création/modification (futur : `CreatedBy`, `ModifiedBy`, `ModifiedAt`).
