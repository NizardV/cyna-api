# 📚 Index de la Documentation — Cyna API

Ce dossier centralise toute la documentation technique du projet **Cyna API** (back-end ASP.NET Core / .NET 10). Les documents sont numérotés dans un ordre de lecture logique, du plus général au plus spécifique.

---

## 🆕 Documents transverses (nouveaux)

| # | Document | Contenu |
|---|---|---|
| 00 | [`00-Architecture-Generale.md`](./00-Architecture-Generale.md) | Couches du projet, injection de dépendances, pipeline HTTP, CORS, build/déploiement Docker & CI/CD |
| 01 | [`01-Authentification-JWT-2FA.md`](./01-Authentification-JWT-2FA.md) | Cookies JWT, refresh token, policy `AdminOnly`, double authentification TOTP des admins (mode bootstrap) |
| 02 | [`02-Email-OTP.md`](./02-Email-OTP.md) | Envoi d'emails via Resend, génération d'OTP, vérification d'email, reset de mot de passe |
| 03 | [`03-Gestion-Utilisateurs.md`](./03-Gestion-Utilisateurs.md) | Profil utilisateur, administration des comptes (désactivation, rôles) |
| 04 | [`04-Catalogue-Recherche.md`](./04-Catalogue-Recherche.md) | Navigation catalogue par catégorie, recherche globale, algorithme "Catalog Priority", calcul de prix |
| 05 | [`05-Panier-Commandes.md`](./05-Panier-Commandes.md) | Tarification par paliers, cycle panier → commande → abonnement, protections de suppression |
| 06 | [`06-Categories.md`](./06-Categories.md) | CRUD catégories, traductions, génération de slug |
| 07 | [`07-Dashboard-Statistiques.md`](./07-Dashboard-Statistiques.md) | Statistiques admin, filtrage temporel, mode mock (Bogus) |
| 08 | [`08-Base-de-donnees.md`](./08-Base-de-donnees.md) | Modèle de données EF Core, index, `DeleteBehavior.Restrict`, intercepteur de requêtes lentes |
| 09 | [`09-CMS-PageAccueil.md`](./09-CMS-PageAccueil.md) | Pattern BFF de la page d'accueil, agrégation des quatre blocs de contenu |

## 📄 Documents existants (historiques, conservés et référencés)

| Document | Contenu |
|---|---|
| [`CatalogCategory-page.md`](./CatalogCategory-page.md) | Détail historique de l'implémentation du catalogue par catégorie |
| [`Homepage.md`](./Homepage.md) | Détail historique de la page d'accueil, état d'avancement fonctionnel |
| [`ProductAdmin-CRUD.md`](./ProductAdmin-CRUD.md) | CRUD complet des produits back-office (DTOs, flux, tests d'intégration) |
| [`ProductDetails-page.md`](./ProductDetails-page.md) | Page détail produit et produits similaires |

---

## 🗺️ Carte de lecture par besoin

**« Je veux comprendre comment l'API est structurée et déployée »**
→ `00-Architecture-Generale.md`

**« Je veux comprendre la connexion, les tokens, le 2FA admin »**
→ `01-Authentification-JWT-2FA.md` → `02-Email-OTP.md`

**« Je veux comprendre comment un produit arrive jusqu'au panier et à la facture »**
→ `04-Catalogue-Recherche.md` → `05-Panier-Commandes.md` → `ProductAdmin-CRUD.md`

**« Je veux comprendre le modèle de données et ses contraintes »**
→ `08-Base-de-donnees.md`

**« Je veux comprendre le back-office admin (utilisateurs, catégories, produits, dashboard) »**
→ `03-Gestion-Utilisateurs.md` → `06-Categories.md` → `ProductAdmin-CRUD.md` → `07-Dashboard-Statistiques.md`

---

## 🚨 Synthèse des points d'attention critiques relevés

Cette section recense, en un seul endroit, les anomalies et dettes techniques identifiées pendant la rédaction de cette documentation — à traiter avant une mise en production complète.

| Sévérité | Sujet | Détail | Document |
|---|---|---|---|
| 🔴 Critique | Dashboard non protégé | `[Authorize(Roles = "Admin,SuperAdmin")]` est commenté sur `DashboardController` — statistiques accessibles sans authentification | `07-Dashboard-Statistiques.md` |
| 🔴 Critique | Pas de vérification de paiement serveur | `Order.Status = Paid` est posé sans vérifier le paiement Stripe réellement effectué | `05-Panier-Commandes.md` |
| 🟠 Élevé | Cookies non sécurisés | `Secure = false` sur les cookies JWT — doit être `true` derrière HTTPS en production | `01-Authentification-JWT-2FA.md` |
| 🟠 Élevé | Rôles mal nommés sur certaines routes | `[Authorize(Roles = "Admin")]` (nom enum C#) ne correspond pas au claim `role` (libellé `[Description]`, ex. `"Administrateur"`) | `06-Categories.md`, `07-Dashboard-Statistiques.md` |
| 🟡 Moyen | Namespace obsolète dans l'intercepteur EF | `EfSlowQueryInterceptor` recherche `"Webzine.Repository"` au lieu de `"Infrastructure.Repositories"` — log d'appelant toujours `"inconnu"` | `08-Base-de-donnees.md` |
| 🟡 Moyen | Pas de transaction explicite | Création de commande multi-étapes sans `BeginTransactionAsync` | `05-Panier-Commandes.md`, `08-Base-de-donnees.md` |
| 🟢 Mineur | Slug catégorie mutable | Contrairement aux produits (slug immuable), le slug de catégorie peut être modifié et casser des liens externes | `06-Categories.md` |
| 🟢 Mineur | Fichier vide oublié | `Application/Services/temp.cs` est vide, à supprimer | `08-Base-de-donnees.md` |

---

*Documentation maintenue dans le dossier `Docs/` du dépôt `Cyna-Api`. Pour toute modification de comportement métier, mettre à jour le document concerné en même temps que le code.*