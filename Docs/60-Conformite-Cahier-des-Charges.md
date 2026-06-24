# Conformité au Cahier des Charges & Respect des Délais — Cyna API

## 🎯 Objectif du document

Démontrer l'**adéquation entre les livrables et les exigences initiales**, via une **matrice de
traçabilité** (exigence → implémentation → documentation → test → état), et rendre compte du
**respect des délais** et du **périmètre livré**.

> ℹ️ La matrice ci-dessous est construite à partir des **fonctionnalités effectivement livrées** et
> doit être **recoupée avec le cahier des charges officiel** du projet. Chaque ligne renvoie au code
> et au document qui en font foi, afin que le correcteur puisse vérifier chaque exigence.

---

## 1. ✅ Matrice de traçabilité fonctionnelle

Légende état : ✅ Livré · ⚠️ Livré avec réserve (voir note) · 🔲 Partiel / planifié

### 1.1 Authentification & comptes

| Exigence | Implémentation (code) | Doc | Test | État |
|---|---|---|---|---|
| Inscription utilisateur + vérification email | `AuthController`, `AuthService.RegisterAsync` | [01](01-Authentification-JWT-2FA.md) / [02](02-Email-OTP.md) | `AuthServiceTests` | ✅ |
| Connexion / déconnexion (JWT cookies) | `AuthController` `/login` `/logout` | [01](01-Authentification-JWT-2FA.md) | `AuthServiceTests` | ✅ |
| Rafraîchissement de session | `/auth/refresh`, refresh token en base | [01](01-Authentification-JWT-2FA.md) | `AuthServiceTests` (expiré) | ✅ |
| Réinitialisation de mot de passe | `/auth/forgot-password` `/reset-password` | [02](02-Email-OTP.md) | 🔲 plan §6 tests | ✅ |
| 2FA (TOTP) administrateurs | `/auth/admin/login`, `/2fa/setup` `/confirm` | [01](01-Authentification-JWT-2FA.md) §4 | 🔲 planifié | ✅ |
| Gestion de profil | `UserController` `/user/profile` `/password` | [03](03-Gestion-Utilisateurs.md) | `UserControllerTests` | ✅ |
| Administration des comptes (rôles, désactivation) | `AdminUserController` | [03](03-Gestion-Utilisateurs.md) | 🔲 planifié | ✅ |

### 1.2 Catalogue & recherche

| Exigence | Implémentation | Doc | Test | État |
|---|---|---|---|---|
| Catalogue par catégorie (bannière, tri métier) | `CatalogController` | [04](04-Catalogue-Recherche.md) | 🔲 planifié | ✅ |
| Recherche globale multi-filtres + tri | `SearchController` | [04](04-Catalogue-Recherche.md) | `SearchServiceTests` | ✅ |
| Fiche produit + produits similaires | `ProductController` | [ProductDetails](ProductDetails-page.md) | 🔲 planifié | ✅ |
| Multilingue (FR/EN) | Entités `*Translation`, `i18n` | [06](06-Categories.md) / [08](08-Base-de-donnees.md) | — | ✅ |
| CRUD produits (admin) | `ProductController` (AdminOnly) | [ProductAdmin-CRUD](ProductAdmin-CRUD.md) | 🔲 `ProductCrudTests` planifié | ✅ |
| CRUD catégories (admin) | `CategoryController` | [06](06-Categories.md) | 🔲 planifié | ⚠️ voir SEC-04 (rôles) |

### 1.3 Panier, commande, paiement

| Exigence | Implémentation | Doc | Test | État |
|---|---|---|---|---|
| Panier + tarification par paliers dégressifs | `CartController`/`CartService` | [05](05-Panier-Commandes.md) | `CartServiceTests` (4) | ✅ |
| Calcul TVA + récapitulatif | `CartService` | [05](05-Panier-Commandes.md) | `CartServiceTests` | ✅ |
| Création de commande + abonnements | `OrderService` | [05](05-Panier-Commandes.md) | 🔲 planifié | ⚠️ chemin legacy non sécurisé (SEC-02) |
| Paiement Stripe (abonnements) | `CheckoutService`, `PaymentController` | [PAIEMENT-STRIPE](PAIEMENT-STRIPE.md) | 🔲 `PaymentWebhookTests` planifié | ✅ |
| Webhook = source de vérité | `PaymentWebhookService` | [PAIEMENT-STRIPE-API](PAIEMENT-STRIPE-API.md) | 🔲 planifié | ✅ |
| Bascule Mock/Stripe (dev sans réseau) | `IPaymentService` (Mock/Stripe) | [PAIEMENT-STRIPE-CONFIG](PAIEMENT-STRIPE-CONFIG.md) | utilisé par CI | ✅ |
| Facture (PDF) | entité `Invoice`, `PdfUrl` | [05](05-Panier-Commandes.md) | — | ✅ |

### 1.4 Back-office & contenu

| Exigence | Implémentation | Doc | Test | État |
|---|---|---|---|---|
| Tableau de bord statistiques admin | `DashboardController` | [07](07-Dashboard-Statistiques.md) | 🔲 planifié | ⚠️ `[Authorize]` commenté (SEC-01) |
| Mode démo (données factices) | Bogus, `?mock=true` | [07](07-Dashboard-Statistiques.md) | — | ⚠️ à restreindre hors prod (SEC-07) |
| Page d'accueil CMS (carrousel, mission, etc.) | `HomeController` (BFF) | [09](09-CMS-PageAccueil.md) | 🔲 planifié | ✅ |

### 1.5 Exigences techniques transverses

| Exigence | Implémentation | Doc | État |
|---|---|---|---|
| Architecture en couches testable | 5 projets `.csproj` (Clean Archi) | [00](00-Architecture-Generale.md) | ✅ |
| Base relationnelle + migrations | EF Core, SQLite/Postgres | [08](08-Base-de-donnees.md) | ✅ |
| Sécurité (auth, RGPD, secrets) | voir doc dédié | [40](40-Securite-et-Conformite.md) | ⚠️ écarts au registre §4 |
| Conteneurisation & CI/CD | Docker, Azure Pipelines, GHCR, OVH | [00](00-Architecture-Generale.md) | ✅ |
| Documentation technique | dossier `Docs/` (ce livrable) | [README](README.md) | ✅ |
| Tests automatisés | xUnit + Moq (19 tests) + infra intégration | [30](30-Strategie-et-Resultats-de-Tests.md) | ⚠️ à merger + compléter |

---

## 2. 📦 Périmètre livré vs réserves

### Livré et conforme
Authentification complète (dont 2FA admin), catalogue/recherche multilingue, panier à tarification
par paliers, paiement Stripe par webhook, back-office (produits, catégories, utilisateurs, dashboard),
CMS de la page d'accueil, conteneurisation et CI/CD de bout en bout.

### Livré avec réserve (à corriger avant production)
Voir le **registre des vulnérabilités** [`40-Securite-et-Conformite.md`](40-Securite-et-Conformite.md) §4 :
protection du dashboard (SEC-01), sécurisation du chemin de commande legacy (SEC-02), cookies
`Secure` (SEC-03), nommage des rôles (SEC-04).

### Non couvert / hors périmètre API
Demande de devis (quantité au-delà des paliers) — logique portée par le front/commercial ;
infrastructure réseau détaillée (réseau, pare-feu, sauvegardes, auto-scaling) — dépôt **Cyna-Infra**.

---

## 3. 🗓️ Respect des délais & jalons

Le projet est cadencé par les **rendus (Blocs de Compétences)**. Le présent document s'inscrit dans
le **5ᵉ rendu (BC3) — Livrable final technique**.

| Jalon | Objet | État |
|---|---|---|
| Rendus précédents | Conception, premiers modules | Livrés |
| **BC3 — Livrable final technique** | Documentation technique complète (DAT/DCT), tests, sécurité, scalabilité | **Ce livrable** |

### Preuve de cadence de livraison : versioning automatisé

Le respect des délais est **objectivable** par l'historique de livraison continue : le pipeline CD
applique un **versioning sémantique automatique** sur `main` (analyse des messages de commit
conventionnels `feat:` / `fix:` / `feat!:`), crée un **tag Git** `api/vX.Y.Z` et tague l'image
Docker correspondante à **chaque** déploiement de production. L'historique des tags constitue donc
une **trace horodatée** des incréments livrés.

```mermaid
flowchart LR
    C[Commits conventionnels] --> V[Calcul SemVer auto] --> T[Tag api/vX.Y.Z] --> I[Image Docker vX.Y.Z] --> D[Déploiement OVH + health check]
```

> Pour consulter l'historique réel des versions livrées : `git tag --list 'api/v*' --sort=-v:refname`.

---

## 🔗 Documents liés

* [`README.md`](README.md) — index et mapping à la grille d'évaluation
* [`40-Securite-et-Conformite.md`](40-Securite-et-Conformite.md) — réserves de sécurité
* [`30-Strategie-et-Resultats-de-Tests.md`](30-Strategie-et-Resultats-de-Tests.md) — couverture de test
* [`70-Gouvernance-et-Roadmap.md`](70-Gouvernance-et-Roadmap.md) — suite à donner
