# Documentation Évolutive - Page Détail Produit & Recommandations

Ce document détaille l'architecture et le flux de données pour la page Produit, qui se divise en deux requêtes distinctes pour optimiser le chargement Front-End (Lazy Loading) :
1. **Les détails complets** : `GET /Product/{id}`
2. **Les recommandations (Similaires)** : `GET /Product/{id}/similar`

---

## 📌 Architecture & Philosophie (Domain-Driven Design)

* **Découplage strict** : La page produit possède son propre écosystème de DTOs dans le dossier `Domain/Dto/Product`. **Aucun DTO du Catalogue n'est réutilisé ici.**
* **Objectifs de la route Détails (`/{id}`)** : Fournir au Front-End une vue profonde et complète d'un produit unique (spécifications techniques, galerie d'images, grilles tarifaires détaillées par paliers) en une seule requête optimisée.
* **Objectifs de la route Similaires (`/{id}/similar`)** : Fournir 6 cartes de produits allégées. Le format de sortie a été rigoureusement aligné sur le mock Front-End (propriétés aplaties, status en minuscules) pour garantir une intégration "Plug & Play" sans surcharger la bande passante (zéro sur-chargement / *over-fetching*).

---

## 🔄 Flux Technique et Couches

### 1. Le Contrôleur (`ProductController`)
* **Rôle** : Point d'entrée HTTP.
* **`GET /{id}`** : Intercepte l'ID. Renvoie `404 Not Found` si introuvable, sinon `200 OK` avec l'arbre JSON complet.
* **`GET /{id}/similar`** : Renvoie les recommandations. Si aucun produit similaire n'est trouvé, renvoie un tableau vide `[]` (`200 OK`) plutôt qu'une erreur `404`, respectant ainsi les conventions REST pour les listes.

### 2. Le Service (`ProductService`)
* **Rôle** : Orchestration, Mappage (LINQ-to-Objects) et logique métier de présentation.
* **Logique Détails** : Matérialise les listes complexes en mémoire via `.ToList()` pour sécuriser la sérialisation JSON finale.
* **Logique Similaires** : 
  * Calcule le prix d'appel (`Price`) en cherchant la valeur unitaire (`PricePerUnit`) la plus basse à travers tous les paliers disponibles.
  * Applique une troncature intelligente sur la description (100 caractères max).
  * Formate le statut (ex: `Active` devient `available`) pour correspondre aux attentes strictes du Front-End.

### 3. Le Repository (`ProductRepository`)
* **Rôle** : Requêtage SQL via Entity Framework Core avec optimisation `.AsNoTracking()`.
* **Requête Détails** : Charge un arbre relationnel profond (Traductions, Catégorie, Images, PricingPlans, PricingTiers).
* **Requête Similaires (Algorithme de Fallback)** :
  1. Identifie la catégorie du produit source.
  2. Tente de récupérer 6 produits de cette **même catégorie**, triés par disponibilité (`Available` en priorité).
  3. **Plan de secours (Fallback)** : S'il y a moins de 6 produits dans cette catégorie, le Repository effectue une seconde sélection sur les **autres catégories** pour combler le vide exact (ex: `Take(6 - count)`).
  * *Note de conception* : Le tri aléatoire a été volontairement écarté de la couche SQL pour garantir une compatibilité universelle et des performances maximales entre SQLite (Dev) et PostgreSQL (Prod), sans charger de données inutiles en RAM.

---

## 📦 Structure des Données (Les DTOs)

### 1. DTO Principal : `ProductDetailsDto` (Route `/{id}`)
Objet racine structuré en "poupées russes" :
* **Base** : `Id`, `Slug`, `Name`, `Description`, `TechnicalSpecs`, `Status`.
* **`Category`** (`ProductCategoryDto`) : Informations légères pour le fil d'ariane (Breadcrumb).
* **`Images`** (`IEnumerable<string>`) : Tableau d'URLs pour le carrousel.
* **`PricingPlans`** (`IEnumerable<ProductPricingPlanDto>`) : Les onglets de tarification (Mensuel, Annuel).
  * ↳ **`PricingTiers`** (`IEnumerable<ProductPricingTierDto>`) : Les paliers dégressifs pour générer les tableaux de prix au Front.

### 2. DTO Secondaire : `ProductSimilarDto` (Route `/{id}/similar`)
Objet "plat" et ultra-léger pour l'affichage de cartes :
* `Id`, `Slug`, `Name`, `Description` (tronquée), `Status` (minuscules).
* `ImageUrl` (Une seule image, la principale).
* `Price` (Le prix d'appel le plus bas trouvé dans l'arbre tarifaire).

---

## 🧪 Guide de Test (Swagger / Scalar)

1. Lancer l'API en environnement de développement (le `--seed` génère les produits).
2. Ouvrir l'outil de documentation web (Scalar ou Swagger).
3. **Tester les détails** : Exécuter la route `GET /Product/{id}` avec un ID existant (ex: `1`). Vérifier la présence de l'arbre tarifaire complet (`pricingPlans` > `pricingTiers`).
4. **Tester les recommandations** : Exécuter la route `GET /Product/{id}/similar` avec le même ID. 
   * Vérifier que le tableau contient exactement 6 produits.
   * Vérifier que le produit source (ID `1`) n'est pas présent dans la liste.
   * Vérifier la présence d'un prix unique `price` et d'une description courte.