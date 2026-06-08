# Documentation Évolutive - Page d'Accueil (Route `/Home`)

Ce document centralise l'architecture, le flux de données et l'état d'avancement des fonctionnalités de la page d'accueil (Pattern BFF - Backend For Frontend). 

---

## 📌 Architecture Générale & Route
* **Route unique** : `GET /Home` (gérée par le `HomeController` dans le projet **Api**).
* **Paramètre** : `?locale=X` (type `LocaleLang` : `Fr = 0`, `En = 1`).
* **Objectif** : Renvoyer l'intégralité des données de la Home en une seule requête HTTP pour optimiser le Front-End React.

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
 
---

## 🧪 Guide de Test (Swagger)

1. Lancer l'API avec l'argument `--seed` pour remplir la base SQLite locale.
2. Ouvrir Swagger à la racine.
3. Exécuter un `GET` sur `/Home`.

