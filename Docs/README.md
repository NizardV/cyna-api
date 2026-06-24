# 📘 Livrable Final Technique (DAT/DCT) — Cyna API

**Projet** : Cyna — plateforme e-commerce de services de cybersécurité
**Composant** : Back-end / API REST (ASP.NET Core · .NET 10)
**Document** : Dossier d'Architecture Technique & de Conception Technique (DAT/DCT)
**Dépôt** : `Cyna-Api` · **Rendu** : BC3 — Livrable final technique

---

## 1. 📖 Introduction

Ce document constitue le **livrable technique final** du back-end Cyna. Il s'adresse à un
**technicien ou un ingénieur** souhaitant comprendre, maintenir ou faire évoluer le système :
architecture, modules métier, schémas, base de données, tests, sécurité/conformité, performance et
gouvernance y sont décrits.

La documentation est **versionnée dans le dépôt** (dossier `Docs/`) : elle évolue avec le code et est
revue en *pull request* au même titre que celui-ci. Tous les schémas sont écrits en **Mermaid**
(rendu natif sur Azure DevOps / GitHub) afin d'être maintenus *as code*.

**Comment lire ce document** : suivez l'ordre numéroté (du général au spécifique), ou utilisez la
**carte de lecture par besoin** (§4) et le **mapping vers la grille d'évaluation** (§5).

---

## 2. 🗂️ Sommaire structuré (DAT/DCT)

### Partie I — Présentation & architecture
| # | Document | Contenu |
|---|---|---|
| 00 | [Architecture Générale](00-Architecture-Generale.md) | Couches, injection de dépendances, pipeline HTTP, CORS, build & déploiement Docker / CI-CD |

### Partie II — Description technique des modules
| # | Document | Contenu |
|---|---|---|
| 01 | [Authentification JWT & 2FA](01-Authentification-JWT-2FA.md) | Cookies JWT, refresh token, policy `AdminOnly`, 2FA TOTP admin (bootstrap) |
| 02 | [Email & OTP](02-Email-OTP.md) | Resend, génération OTP, vérification email, reset mot de passe |
| 03 | [Gestion des utilisateurs](03-Gestion-Utilisateurs.md) | Profil, administration des comptes (rôles, désactivation) |
| 04 | [Catalogue & recherche](04-Catalogue-Recherche.md) | Navigation par catégorie, recherche globale, tri « Catalog Priority », prix d'appel |
| 05 | [Panier & commandes](05-Panier-Commandes.md) | Tarification par paliers, cycle panier → commande → abonnement |
| 06 | [Catégories](06-Categories.md) | CRUD catégories, traductions, slug |
| 07 | [Dashboard & statistiques](07-Dashboard-Statistiques.md) | Statistiques admin, filtrage temporel, mode mock (Bogus) |
| 08 | [Base de données](08-Base-de-donnees.md) | Modèle EF Core, index, `DeleteBehavior.Restrict`, intercepteur de requêtes lentes |
| 09 | [CMS / Page d'accueil](09-CMS-PageAccueil.md) | Pattern BFF, agrégation des 4 blocs de contenu |
| 10 | [Paiement Stripe](PAIEMENT-STRIPE.md) | Vue d'ensemble · [API/Webhook](PAIEMENT-STRIPE-API.md) · [Config/Secrets](PAIEMENT-STRIPE-CONFIG.md) · [Tests](PAIEMENT-STRIPE-TEST.md) |

### Partie III — Schémas, tests, sécurité, performance (volets transverses du DAT/DCT)
| # | Document | Contenu |
|---|---|---|
| 20 | [Schémas techniques](20-Schemas-Techniques.md) | Contexte, **déploiement/réseau**, composants, **classes UML**, ERD, séquences, états |
| 30 | [Stratégie & résultats de tests](30-Strategie-et-Resultats-de-Tests.md) | Pyramide, méthodologie AAA, inventaire des 19 tests + résultats, plans sécu/perf/résilience, CI |
| 40 | [Sécurité & conformité](40-Securite-et-Conformite.md) | Mesures de sécurité, **RGPD**, **ISO 27001**, registre des vulnérabilités |
| 50 | [Scalabilité & performance](50-Scalabilite-et-Performance.md) | Design stateless, monitoring, health check, optimisations, trajectoire d'auto-scaling |

### Partie IV — Conformité & pilotage
| # | Document | Contenu |
|---|---|---|
| 60 | [Conformité au cahier des charges](60-Conformite-Cahier-des-Charges.md) | Matrice de traçabilité exigences → code → doc → test, respect des délais |
| 70 | [Gouvernance & roadmap](70-Gouvernance-et-Roadmap.md) | Git flow, CI/CD, SemVer, dette technique, roadmap court/moyen/long terme |

### Annexes — Documents de détail (historiques, conservés)
| Document | Contenu |
|---|---|
| [CRUD Admin Produits](ProductAdmin-CRUD.md) | DTOs, flux et logique métier du CRUD produit |
| [Page détail produit](ProductDetails-page.md) | Fiche produit & produits similaires |
| [Catalogue par catégorie](CatalogCategory-page.md) | Détail historique d'implémentation |
| [Page d'accueil](Homepage.md) | Détail historique, état d'avancement fonctionnel |
| [INSTALLATION.md](../INSTALLATION.md) | Procédure d'installation / contribution (racine du dépôt) |

---

## 3. 🚀 Pour démarrer rapidement

| Je veux… | Aller à |
|---|---|
| Installer le projet en local | [INSTALLATION.md](../INSTALLATION.md) |
| Comprendre l'architecture & le déploiement | [00](00-Architecture-Generale.md) → [20](20-Schemas-Techniques.md) |
| Comprendre la sécurité & la conformité | [40](40-Securite-et-Conformite.md) |
| Lancer / comprendre les tests | [30](30-Strategie-et-Resultats-de-Tests.md) |

---

## 4. 🧭 Carte de lecture par besoin

- **« Comprendre comment l'API est structurée et déployée »** → [00](00-Architecture-Generale.md) puis [20](20-Schemas-Techniques.md)
- **« Comprendre connexion, tokens, 2FA »** → [01](01-Authentification-JWT-2FA.md) → [02](02-Email-OTP.md)
- **« Suivre un produit jusqu'au panier et à la facture »** → [04](04-Catalogue-Recherche.md) → [05](05-Panier-Commandes.md) → [10](PAIEMENT-STRIPE.md)
- **« Comprendre le modèle de données »** → [08](08-Base-de-donnees.md) (+ ERD/UML dans [20](20-Schemas-Techniques.md))
- **« Comprendre le back-office admin »** → [03](03-Gestion-Utilisateurs.md) → [06](06-Categories.md) → [ProductAdmin-CRUD](ProductAdmin-CRUD.md) → [07](07-Dashboard-Statistiques.md)

---

## 5. ✅ Mapping vers la grille d'évaluation BC3

Correspondance directe entre les **critères du livrable final** et les sections de cette documentation,
pour faciliter l'évaluation.

| Critère d'évaluation (pts) | Où c'est traité |
|---|---|
| **Précision technique du livrable final** (10) | Modules [00](00-Architecture-Generale.md)→[09](09-CMS-PageAccueil.md), [10](PAIEMENT-STRIPE.md), [ProductAdmin-CRUD](ProductAdmin-CRUD.md) (configs, implémentations détaillées, conformité aux specs) |
| **Pertinence & clarté des schémas techniques** (10) | [20 — Schémas techniques](20-Schemas-Techniques.md) : déploiement/réseau, composants, **UML classes**, ERD, séquences, états |
| **Qualité des tests & résultats (DAT/DCT)** (10) | [30 — Stratégie & résultats de tests](30-Strategie-et-Resultats-de-Tests.md) : méthodologie, inventaire + résultats, sécu/résilience/perf |
| **Organisation & structuration du document** (5) | Le présent index (intro, sommaire, mapping, carte de lecture) + structure numérotée du dossier `Docs/` |
| **Gestion de la sécurité & conformité** (5) | [40 — Sécurité & conformité](40-Securite-et-Conformite.md) : mesures, **RGPD**, **ISO 27001**, registre de vulnérabilités |
| **Scalabilité & performance** (5) | [50 — Scalabilité & performance](50-Scalabilite-et-Performance.md) : stateless, monitoring, auto-scaling, ressources |
| **Respect des délais & conformité aux exigences** (3) | [60 — Conformité au cahier des charges](60-Conformite-Cahier-des-Charges.md) : matrice de traçabilité + jalons |
| **Gouvernance & vision d'évolution** (2) | [70 — Gouvernance & roadmap](70-Gouvernance-et-Roadmap.md) : gouvernance, dette, roadmap |

---

## 6. 🚨 Synthèse des points d'attention critiques

Le registre complet (sévérité, impact, remédiation) est tenu dans
[`40-Securite-et-Conformite.md`](40-Securite-et-Conformite.md) §4. Extrait des plus sensibles :

| Sévérité | Sujet | Réf. |
|---|---|---|
| 🔴 Critique | Dashboard non protégé (`[Authorize]` commenté) | [SEC-01](40-Securite-et-Conformite.md) · [07](07-Dashboard-Statistiques.md) |
| 🔴 Critique | Chemin de commande legacy : `Paid` sans vérif. paiement | [SEC-02](40-Securite-et-Conformite.md) · [05](05-Panier-Commandes.md) |
| 🟠 Élevé | Cookies `Secure=false` (à passer `true` en prod HTTPS) | [SEC-03](40-Securite-et-Conformite.md) · [01](01-Authentification-JWT-2FA.md) |
| 🟠 Élevé | Nommage des rôles (`Roles="Admin"` ≠ claim `role`) | [SEC-04](40-Securite-et-Conformite.md) · [06](06-Categories.md) |

> La priorisation et le calendrier de correction figurent dans la roadmap
> [`70-Gouvernance-et-Roadmap.md`](70-Gouvernance-et-Roadmap.md) §3.

---

## 7. ℹ️ Conventions & périmètre

- **Périmètre** : ce livrable couvre le **back-end (API)**. L'infrastructure détaillée (réseau,
  pare-feu, sauvegardes, auto-scaling) relève du dépôt **Cyna-Infra** et sera documentée séparément ;
  les fronts web et mobile vivent dans **Cyna-Web** et **Cyna-App**.
- **Transparence** : la documentation distingue toujours ce qui est **implémenté** de ce qui est
  **planifié**, et tient un registre honnête des écarts (sécurité, dette).
- **Maintenance** : toute modification de comportement métier doit mettre à jour le document concerné
  dans la **même pull request**.

---

*Documentation maintenue dans `Docs/`. Pour la procédure d'installation locale, voir
[`INSTALLATION.md`](../INSTALLATION.md).*
