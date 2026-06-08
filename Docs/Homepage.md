# Documentation Évolutive - Page d'Accueil (Route `/Home`)

Ce document centralise l'architecture, le flux de données et l'état d'avancement des fonctionnalités de la page d'accueil (Pattern BFF - Backend For Frontend). 

---

## 📌 Architecture Générale & Route
* **Route unique** : `GET /Home` (gérée par le `HomeController` dans le projet **Api**).
* **Paramètre** : `?locale=X` (type `LocaleLang` : `Fr = 0`, `En = 1`).
* **Objectif** : Renvoyer l'intégralité des données de la Home en une seule requête HTTP pour optimiser le Front-End React.
* **Configuration Swagger :** Pour supporter l'existence de DTOs homonymes dans des namespaces différents (ex: `Home.CategoryDto` vs `Catalog.CategoryDto`), Swagger a été configuré avec `CustomSchemaIds(type => type.FullName)` dans le `Program.cs`.

---

## 🔄 État d'Avancement des Fonctionnalités

### 1. 🎪 Le Carrousel 
* **Statut** :  Opérationnel ✅
* **Flux Technique** :
  * `Infrastructure > ICarouselRepository` & `CarouselRepository` : Requête SQL optimisée avec `.AsNoTracking()` et inclusion filtrée de la langue.
  * `Application > ICmsService` & `CmsService` : Reçoit les entités, applique les logs d'anomalies (`ILogger`), et mappe vers le DTO.
  * `Application > Dto > Home > CarouselSlideDto` : Structure finale aplatie (`Id`, `ImageUrl`, `Title`, `Subtitle`, `ButtonText`).

### 2. 📝 Le Texte Fixe de Mission
* **Statut** : Opérationnel ✅
* **Flux Technique** :
  * `Infrastructure > ISiteSetting` (Interface) & `SiteSettingRepository` : Lecture générique de type Clé-Valeur dans la table `SiteSettings` avec filtrage de la langue sur `SettingKey`.
  * `Application > ICmsService` & `CmsService` : Demande explicitement la clé métier `"homepage_mission_text"`. Intègre un log d'avertissement (`LogWarning`) si le texte est manquant en base.
  * `Application > Dto > Home > HomePageDto` : Exposition de la propriété `MissionText` directement à la racine du DTO global de la page d'accueil.
 
### 3. 🛍️ Les Catégories 
* **Statut** : Opérationnel ✅
* **Flux Technique** :
  * `Infrastructure > ICategoryRepository` : Extraction de la table `Categories` avec `.Include()` des traductions et tri SQL par `.OrderBy(c => c.DisplayOrder)`.
  * `Application > ICmsService` : Mappage vers le DTO allégé `CategoryDto` (sans propriétés de tri ou d'ID exposées).
  * `Api > HomeController` : Consolidation finale dans l'objet de réponse `HomePageDto`.
---

## 🧪 Guide de Test (Swagger)

1. Lancer l'API avec l'argument `--seed` pour remplir la base SQLite locale.
2. Ouvrir Swagger à la racine.
3. Exécuter un `GET` sur `/Home`.

