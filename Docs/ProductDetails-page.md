# Documentation Évolutive - Page Détail Produit (Route `/Product/{id}`)

Ce document détaille l'architecture et le flux de données pour la récupération des informations complètes d'un produit (Page Produit).

---

## 📌 Architecture & Philosophie (Domain-Driven Design)
* **Route** : `GET /Product/{id}?locale=X` (gérée par le `ProductController`).
* **Découplage strict** : La page produit possède son propre écosystème de DTOs dans le dossier `Domain/Dto/Product`. **Aucun DTO du Catalogue n'est réutilisé ici.**
* **Objectif** : Fournir au Front-End une vue profonde et complète d'un produit unique (spécifications techniques, galerie d'images, grilles tarifaires détaillées par paliers) en une seule requête optimisée.

---

## 🔄 Flux Technique et Couches

### 1. Le Contrôleur (`ProductController`)
* **Rôle** : Point d'entrée HTTP.
* **Comportement** : Intercepte l'ID et la langue (par défaut : `fr`). Renvoie une erreur `404 Not Found` si le produit n'existe pas ou n'est pas actif, sinon renvoie un statut `200 OK` avec l'arbre JSON complet.

### 2. Le Service (`ProductService`)
* **Rôle** : Orchestration et Mappage (LINQ-to-Objects).
* **Particularité** : C'est ici que les entités de base de données sont transformées en DTOs.
* **Point d'attention sur les `.ToList()`** : Les `.ToList()` sont utilisés intentionnellement à la fin des `.Select()` imbriqués pour **matérialiser** les listes en mémoire (RAM) et éviter l'exécution différée (Lazy Evaluation) lors de la sérialisation JSON finale.

### 3. Le Repository (`ProductRepository`)
* **Rôle** : Requêtage SQL via Entity Framework Core.
* **Optimisation** : Utilisation stricte de `.AsNoTracking()` pour des performances maximales en lecture seule.
* **Jointures (`Include`)** : La requête charge un arbre relationnel profond en une seule fois :
  * Les traductions du produit (filtrées par langue).
  * La catégorie parente et sa traduction (pour le fil d'ariane).
  * Les images (triées par `DisplayOrder`).
  * Les plans tarifaires (`PricingPlans`) avec leurs paliers respectifs (`PricingTiers` triés par quantité minimum).

---

## 📦 Structure des Données (Les DTOs)

Le Front-End reçoit un objet racine `ProductDetailsDto` structuré en "poupées russes" pour faciliter le rendu visuel :

1. **Informations de base** : `Id`, `Slug`, `Name`, `Description`, `TechnicalSpecs`, `Status`.
2. **`Category`** (`ProductCategoryDto`) : Informations légères pour générer le fil d'ariane (Breadcrumb).
3. **`Images`** (`IEnumerable<string>`) : Tableau simple d'URLs pour le carrousel/galerie de la page.
4. **`PricingPlans`** (`IEnumerable<ProductPricingPlanDto>`) : Contient les différents onglets de prix (ex: Mensuel, Annuel).
   * ↳ **`PricingTiers`** (`IEnumerable<ProductPricingTierDto>`) : Imbriqués dans chaque plan, ces objets permettent au Front-End de dessiner facilement les tableaux de prix dégressifs (ex: de 1 à 10 utilisateurs, de 11 à 50, etc.).

---

## 🧪 Guide de Test (Swagger / Scalar)

1. Lancer l'API en environnement de développement (le `--seed` génère les produits).
2. Ouvrir l'outil de documentation web (Scalar ou Swagger).
3. (Optionnel) Exécuter la route `GET /Home` pour repérer un `id` de produit valide dans le bloc `topProducts`.
4. Exécuter la route `GET /Product/{id}` avec un ID existant (ex: `1`, `2`...).
5. Vérifier la présence de l'arbre tarifaire (`pricingPlans` > `pricingTiers`) dans la réponse JSON.