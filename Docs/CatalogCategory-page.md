# Documentation API - Endpoint Catalogue par Catégorie

Ce document détaille l'implémentation côté Back-End de la fonctionnalité "Catalogue par Catégorie". 
L'objectif est de fournir à l'interface client (Front-End) les données nécessaires pour afficher une page dédiée à une catégorie spécifique (bannière) tout en listant ses produits avec un tri métier strict.

---

## 🏗️ 1. Architecture et Choix Techniques


### Séparation des responsabilités :
* **Nouveau Contrôleur (`CatalogController`)** : Dédié à la navigation et à l'exploration du catalogue, séparé de la recherche globale et de l'administration des produits (`ProductController`).
* **Héritage des DTOs** : Le DTO de réponse réutilise la structure de pagination existante pour éviter la duplication de code.

---

## 📂 2. Fichiers Modifiés et Créés

### A. Le DTO (`Domain.Dto.Catalog.CategoryCatalogPageDto`)
Création d'un nouvel objet de transfert héritant de `CatalogPageDto`.
* **Pourquoi ?** Permet de conserver les métadonnées de pagination et la liste des `ProductDto` existantes, tout en y ajoutant les informations spécifiques à l'en-tête de la page catalogue (Nom, Description, Image de la bannière).

### B. Le Contrôleur (`Api.Controllers.CatalogController`)
* **Route :** `GET /Catalog/category/{slug}`
* **Rôle :** Point d'entrée de l'API. Valide les paramètres de pagination (page, pageSize) et intercepte les exceptions métier (ex: `KeyNotFoundException`) pour renvoyer proprement une erreur `404 Not Found`.
* **Documentation :** Entièrement documenté pour Swagger via les attributs `[ProducesResponseType]` (200, 400, 404). Intégration de `NLog` pour la traçabilité des requêtes entrantes et des erreurs de validation.

### C. Le Service (`Application.Services.CatalogService`)
* **Nouvelle méthode :** `GetCategoryCatalogAsync`
* **Rôle :** Orchestre l'appel au Repository. 
* **Logique métier :** * Gère l'exception si la catégorie demandée via le slug n'existe pas.
  * Effectue le calcul du prix d'appel (`lowest price`) en parcourant tous les plans tarifaires (`PricingPlans`) et tous les paliers (`PricingTiers`), en prenant en compte les pourcentages de réduction.
  * Ajout de logs (`_logger.Info`, `_logger.Warn`) pour suivre l'activité métier.

### D. Le Repository (`Infrastructure.Repositories.CatalogRepository`)
* **Nouvelle méthode :** `GetCategoryCatalogAsync` (déclarée dans `ICatalogRepository`)
* **Rôle :** Requêtage Entity Framework Core avec optimisation `.AsNoTracking()`.
* **Règles appliquées :**
  1. Filtre strict sur la catégorie demandée.
  2. Application des filtres optionnels transmis par le Front-End (Recherche textuelle `q`, Budget maximum `maxPrice`, Disponibilité `available`).
  3. **Tri métier "Catalog Priority" :** Les produits sont triés de manière rigide (et non dynamique) :
     * Disponibles en premier.
     * Mis en avant (`IsFeatured`) en second.
     * Ordre d'affichage manuel (`DisplayOrder`).
     * Ordre de création (`Id`).

