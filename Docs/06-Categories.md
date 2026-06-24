# Gestion des Catégories — Cyna API

## 🎯 Objectif du document

Documenter le CRUD des catégories de produits (`CategoryController`), ses règles de validation (locales, unicité du slug) et son articulation avec les traductions multilingues.

---

## 🌍 1. Modèle multilingue

Une `Category` possède une collection de `CategoryTranslation` (une par `LocaleLang` : `Fr` ou `En`), chacune portant `Name` et `Description`. Le DTO `CategoryDto` **aplati** résout systématiquement :

```csharp
var frTr = c.Translations.FirstOrDefault(t => t.Locale == LocaleLang.Fr)
        ?? c.Translations.FirstOrDefault();
```

→ priorité au français, sinon la première traduction disponible (filet de sécurité si le français n'a pas encore été saisi).

Le DTO inclut aussi `Translations` (toutes les traductions, format `{locale: "fr"|"en", name, description}`) — utilisé par le formulaire d'édition admin pour afficher/modifier les deux langues simultanément.

---

## 🔐 2. Sécurité des routes

```csharp
[Route("categories")]
[Authorize(Roles = "Admin")]   // appliqué au niveau du contrôleur
```

⚠️ **Incohérence à noter** : l'attribut de classe utilise `Roles = "Admin"` (nom C# de l'enum `UserRole.Admin`), alors que la policy globale `AdminOnly` (voir `01-Authentification-JWT-2FA.md`) et le claim JWT `role` utilisent les **libellés `[Description]`** (`"Administrateur"`, `"Super Administrateur"`). Étant donné que `RoleClaimType = "role"` et que le claim contient la description (`GetEnumDescription()`), `Roles = "Admin"` ne matchera **jamais** le claim réel — sauf si ASP.NET Core résout ce rôle autrement. Ce point mérite une vérification/test d'intégration dédié (`CategoryController` pourrait être accessible à personne, ou au contraire mal protégé, selon le comportement réel observé). Les routes de lecture (`GET`) sont explicitement ouvertes via `[AllowAnonymous]`, donc l'impact se limiterait au CRUD d'écriture (`POST`/`PUT`/`DELETE`).

| Route | Méthode | Accès |
|---|---|---|
| `/categories` | GET | Public (`[AllowAnonymous]`) |
| `/categories/search` | GET | Public (`[AllowAnonymous]`) |
| `/categories/{id}` | GET | Public (`[AllowAnonymous]`) |
| `/categories` | POST | Admin (cf. remarque ci-dessus) |
| `/categories/{id}` | PUT | Admin |
| `/categories/{id}` | DELETE | Admin |

---

## ✅ 3. Validation des traductions (`ValidateTranslationLocales`)

Avant tout `Create`/`Update`, le contrôleur exécute une validation **dédiée**, en plus de `ModelState.IsValid` :

```csharp
foreach (var t in translations)
{
    if (i18n.ParseLocale(t.Locale) is null)
        return $"Locale inconnue « {t.Locale} ». Valeurs acceptées : fr, en.";
    if (!seen.Add(t.Locale.ToLower()))
        return $"La locale « {t.Locale} » est présente plusieurs fois.";
}
```

Deux règles :
1. La locale doit être `fr` ou `en` (`i18n.ParseLocale`, insensible à la casse).
2. **Pas de doublon** de locale dans une même requête (ex. deux traductions `"fr"` envoyées simultanément).

---

## 🔗 4. Slug — génération et unicité

### Création (`CategoryService.CreateAsync`)

```csharp
var firstName = dto.Translations.FirstOrDefault()?.Name ?? "categorie";
var slug = string.IsNullOrWhiteSpace(dto.Slug)
    ? GenerateSlug(firstName)
    : dto.Slug.Trim().ToLower();

if (await _repo.SlugExistsAsync(slug))
    throw new InvalidOperationException($"Le slug « {slug} » est déjà utilisé.");
```

* Le slug peut être **fourni explicitement** par l'admin, ou **généré automatiquement** depuis le nom de la première traduction si absent.
* `GenerateSlug` : normalisation Unicode (suppression des accents via une série de `Regex.Replace` ciblés sur les voyelles accentuées et le ç), conversion en minuscules, remplacement des espaces par des tirets, suppression des caractères non alphanumériques.
* Collision → `InvalidOperationException` → `400 BadRequest` (et non `409 Conflict`, contrairement au comportement du module Produits — incohérence de convention HTTP entre les deux modules, voir `Docs/ProductAdmin-CRUD.md`).

### Mise à jour (`UpdateAsync`)

Le slug peut être modifié explicitement (`dto.Slug`), avec vérification d'unicité **excluant la catégorie elle-même** (`excludeId: id`) :

```csharp
if (await _repo.SlugExistsAsync(slug, excludeId: id))
    throw new InvalidOperationException(...);
```

> ⚠️ Contrairement au module Produits où le slug est **explicitement immuable** après création (voir `Docs/ProductAdmin-CRUD.md`), les catégories autorisent la modification du slug — ce qui peut **casser des liens externes** existants pointant vers l'ancienne URL de catégorie. À évaluer si ce comportement est volontaire ou s'il doit être aligné avec celui des produits.

---

## 🔄 5. Mise à jour partielle (`UpdateCategoryDto`)

Tous les champs de `UpdateCategoryDto` sont **optionnels** (`string?`, `int?`, `IEnumerable<...>?`) — seuls les champs non-null fournis sont appliqués :

```csharp
if (dto.Slug is not null) { ... }
if (dto.ImageUrl is not null) category.ImageUrl = dto.ImageUrl;
if (dto.DisplayOrder.HasValue) category.DisplayOrder = dto.DisplayOrder.Value;
if (dto.Translations is not null) { /* remplacement complet de la collection */ }
```

⚠️ `Translations`, si fourni, **remplace entièrement** la collection existante (pas de fusion partielle) : envoyer uniquement la traduction `fr` dans un `PUT` supprimera la traduction `en` existante si elle n'est pas réincluse dans le payload.

---

## 📄 6. Liste paginée avec tri (`GET /categories/search`)

| `sortBy` | Tri appliqué |
|---|---|
| `displayOrder` (défaut) | `OrderBy(DisplayOrder)` |
| `name` | `OrderBy(Translations.FirstOrDefault().Name)` |
| `name_desc` | `OrderByDescending(...)` |
| `productCount` | `OrderByDescending(Products.Count)` |

Validation stricte des valeurs acceptées côté contrôleur (`validSortValues.Contains(sortBy)`) → `400` sinon, avec message listant les valeurs valides.

---

## ⚠️ 7. Points d'attention

* **Vérifier le comportement réel de `[Authorize(Roles = "Admin")]`** vu l'incohérence avec le claim `role` basé sur les descriptions d'enum (cf. section 2) — risque de sécurité (routes non protégées comme prévu) ou de régression fonctionnelle (admins légitimes bloqués).
* Incohérence de code HTTP : conflit de slug → `400` ici, vs `409`/`400` selon le contexte dans le module Produits. À harmoniser pour la cohérence de l'API publique.
* Le slug des catégories n'est pas immuable, contrairement aux produits — à documenter explicitement comme un choix assumé ou à corriger.

---

## 🔗 Documents liés

* `01-Authentification-JWT-2FA.md` (rôles et policies)
* `Docs/ProductAdmin-CRUD.md`
* `04-Catalogue-Recherche.md`