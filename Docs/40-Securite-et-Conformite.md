# Sécurité & Conformité — Cyna API

## 🎯 Objectif du document

Justifier les **mesures de sécurité** mises en œuvre dans le back-end Cyna et démontrer la
**conformité réglementaire** (RGPD) et normative (ISO/IEC 27001). Ce document consolide en un seul
endroit ce qui est éparpillé dans les documents fonctionnels, et tient un **registre transparent
des vulnérabilités identifiées** avec leur plan de remédiation.

---

## 1. 🛡️ Mesures de sécurité techniques

### 1.1 Authentification & gestion de session

| Mesure | Mise en œuvre | Bénéfice |
|---|---|---|
| Jetons en **cookies `HttpOnly`** | `cyna_token` (15 min) + `cyna_refresh_token` (24 h), jamais en `localStorage` | Protège du vol de token par **XSS** |
| `SameSite=Strict` | `AuthController.GetCookieOptions` | Réduit le risque **CSRF** |
| Validation JWT stricte | `ValidateIssuer/Audience/Lifetime/SigningKey`, `ClockSkew=0` | Pas de token forgé ni rejoué après expiration |
| **Refresh token opaque** | Chaîne aléatoire (RNG 64 octets), stockée en base, expiration contrôlée | Révocable côté serveur (logout, compte désactivé) |
| Rôle dans claim dédié | `RoleClaimType="role"`, `MapInboundClaims=false` | Contrôle d'accès explicite |

Détail : [`01-Authentification-JWT-2FA.md`](01-Authentification-JWT-2FA.md).

### 1.2 Mots de passe

- Hachage via `PasswordHasher<object>` (**ASP.NET Core Identity**, PBKDF2 salé) — `Tools/HashExstension.cs`.
- **Aucun mot de passe en clair** stocké ou journalisé ; seul `PasswordHash` est persisté.
- Changement de mot de passe : vérification de l'ancien mot de passe obligatoire.
- ⚠️ Politique de longueur **incohérente** : 6 caractères à l'inscription, 8 au reset — à harmoniser (voir registre §4).

### 1.3 Double authentification (2FA / TOTP) des administrateurs

- TOTP RFC 6238 (`Otp.NET`), secret 160 bits base32, fenêtre ±1 période (dérive d'horloge).
- Distinction `TwoFactorSecret` (présent) vs `TwoFactorEnabled` (confirmé) → **mode bootstrap** qui
  empêche de verrouiller un admin hors de son compte.
- Un admin « pleinement configuré » **ne peut pas** contourner le 2FA via le login grand public.

Détail : [`01-Authentification-JWT-2FA.md`](01-Authentification-JWT-2FA.md) §4.

### 1.4 Contrôle d'accès (autorisation)

- Policy `AdminOnly` (`RequireRole("Administrateur", "Super Administrateur")`).
- Routes publiques explicitement `[AllowAnonymous]` ; routes sensibles `[Authorize]` / `AdminOnly`.
- Exclusion de l'admin courant de la liste des utilisateurs (anti auto-verrouillage).

### 1.5 Protection des données métier

| Mesure | Mise en œuvre |
|---|---|
| **Anti-énumération d'emails** | `/auth/forgot-password` renvoie toujours `200` + message générique |
| **OTP cryptographiques** | `RandomNumberGenerator` (jamais `Random`), 6 chiffres, expiration 15/30 min, usage unique |
| **Recalcul serveur des montants** | Les prix sont toujours recalculés depuis le panier ; le front ne peut pas imposer un prix |
| **Webhook = source de vérité** | Une commande n'est `Paid` qu'après webhook Stripe **signé** vérifié (`whsec_`) ; signature invalide → `400` |
| **Intégrité de l'historique** | `DeleteBehavior.Restrict` : produit/plan référencé non supprimable (voir [`08-Base-de-donnees.md`](08-Base-de-donnees.md)) |
| **Snapshot de facturation** | `OrderItem` fige nom/prix au moment de l'achat (intégrité comptable) |

### 1.6 Sécurité réseau & infrastructure

- **HTTPS forcé** (`UseHttpsRedirection`) ; Stripe refuse tout webhook non-HTTPS.
- **CORS restrictif par environnement** (liste blanche d'origines, voir [`20-Schemas-Techniques.md`](20-Schemas-Techniques.md) §2).
- **Conteneur non-root** (`Dockerfile`).
- **Base PostgreSQL non exposée publiquement** (port lié à `127.0.0.1`, accès via réseau Docker interne).
- Documentation API (Scalar/Swagger) **désactivée en production**.

### 1.7 Gestion des secrets

- Secrets (`JwtSettings:Secret`, `Stripe:*`, `Resend:ApiKey`, chaîne de connexion) **hors du dépôt** :
  `appsettings.Development/Staging/Production.json` sont **gitignorés** ; un `*.example` versionné
  sert de modèle.
- En déployé, les secrets sont injectés par **variables d'environnement** (`docker-compose` + `.env`
  non commités, secrets de pipeline Azure DevOps `GHCR_PAT`, `ADO_PAT`).
- Analyse **SonarCloud** en CI (détection de secrets/hotspots).

---

## 2. 📜 Conformité RGPD

### 2.1 Inventaire des données personnelles traitées

| Donnée | Entité / champ | Catégorie | Sensibilité |
|---|---|---|---|
| Email | `User.Email` | Identifiant / contact | Normale |
| Nom, prénom | `User.FirstName`, `User.LastName` | Identité | Normale |
| Mot de passe (haché) | `User.PasswordHash` | Authentifiant | Élevée (haché, jamais en clair) |
| Secret 2FA | `User.TwoFactorSecret` | Authentifiant | Élevée |
| Adresse postale | `Address.*` | Coordonnées | Normale |
| Moyen de paiement (référence) | `PaymentMethod.StripePaymentMethodId`, `CardBrand` | Financier | Élevée (**numéro de carte stocké chez Stripe, pas chez nous**) |
| Identifiant client Stripe | `User.StripeCustomerId` | Financier | Normale |
| Historique de commandes / abonnements | `Order`, `OrderItem`, `Subscription`, `Invoice` | Financier | Élevée |
| Messages de contact / chatbot | `ContactMessage`, `ChatbotMessage` | Contenu utilisateur | Variable |

> 💳 **Aucune donnée de carte bancaire complète n'est stockée par l'API** : la collecte et le
> stockage du numéro de carte sont **délégués à Stripe** (Stripe Elements côté front + tokenisation).
> Cyna ne conserve que des **références** (`pm_…`, `cus_…`) — réduction majeure de la surface PCI-DSS.

### 2.2 Registre des traitements (synthèse)

| Traitement | Finalité | Base légale (RGPD art. 6) | Données | Conservation |
|---|---|---|---|---|
| Création / gestion de compte | Fournir le service | Exécution du contrat | Identité, email, mot de passe | Durée du compte + délai légal |
| Authentification & sécurité | Sécuriser l'accès | Intérêt légitime | Tokens, secret 2FA, logs | Durée de session / vie du token |
| Commandes & facturation | Vendre et facturer | Exécution du contrat / **obligation légale** (facture) | Commandes, factures, adresses | **10 ans** (obligation comptable) |
| Emails transactionnels | Vérif. email, reset MDP | Exécution du contrat | Email, OTP | OTP : 15–30 min |
| Support (contact / chatbot) | Répondre aux demandes | Intérêt légitime / consentement | Messages | À définir (durée utile) |

### 2.3 Droits des personnes — état de prise en charge

| Droit (RGPD) | Pris en charge ? | Mécanisme |
|---|---|---|
| Accès / portabilité | Partiel | `GET /user/profile`, `/user/orders`, `/user/subscriptions` (export structuré à ajouter) |
| Rectification | ✅ | `PUT /user/profile`, `/user/password` |
| Effacement (« droit à l'oubli ») | ⚠️ Partiel | `disable` (désactivation) existe ; **anonymisation/suppression** à formaliser, en conciliant avec l'obligation de conservation des factures |
| Limitation / opposition | ⚠️ À formaliser | Désactivation de compte ; procédure à documenter |
| Information | ✅ (à compléter) | Présent doc + mentions à publier côté front |

> ⚠️ **Tension à arbitrer** : le `DeleteBehavior.Restrict` et l'obligation de conserver les factures
> (10 ans) empêchent une suppression *physique* immédiate. La voie recommandée est
> l'**anonymisation** (dissocier les données d'identité des enregistrements comptables) plutôt que
> la suppression brute. À implémenter (voir [`70-Gouvernance-et-Roadmap.md`](70-Gouvernance-et-Roadmap.md)).

### 2.4 Sous-traitants (RGPD art. 28)

| Sous-traitant | Rôle | Données transmises | Localisation / garanties |
|---|---|---|---|
| **Stripe** | Paiement, abonnements | Données de carte, email, montant | Conforme PCI-DSS ; DPA Stripe |
| **Resend** | Envoi d'emails | Email, contenu transactionnel | DPA fournisseur |
| **OVH** | Hébergement | Toutes (chiffrées au repos selon offre) | Hébergeur UE |

> Un **registre des sous-traitants** et les DPA correspondants doivent être conservés côté
> organisation (hors code).

### 2.5 Principes RGPD appliqués *by design*

- **Minimisation** : seules les données nécessaires sont collectées ; pas de numéro de carte stocké.
- **Sécurité par défaut** : hachage des mots de passe, HTTPS, cookies HttpOnly, secrets hors dépôt.
- **Exactitude** : re-vérification d'email à chaque changement d'adresse.

---

## 3. 🏛️ Conformité ISO/IEC 27001 (mapping Annexe A)

Correspondance entre les mesures du projet et les domaines de contrôle ISO 27001:2022 (Annexe A).
*Le SMSI complet (politique, analyse de risques, audit) relève de l'organisation ; ce tableau
documente la contribution technique de l'API.*

| Contrôle (thème Annexe A) | Mesure Cyna API |
|---|---|
| **A.5 Politiques / organisation** | Conventions de sécurité documentées (ce doc, [`70-Gouvernance-et-Roadmap.md`](70-Gouvernance-et-Roadmap.md)) |
| **A.8 Contrôle d'accès** | JWT + policy `AdminOnly`, 2FA admin, refresh révocable, désactivation de compte |
| **A.8 Authentification sécurisée** | Hachage PBKDF2, TOTP RFC 6238, anti-énumération |
| **A.8 Cryptographie** | TLS/HTTPS, RNG cryptographique (OTP), signatures webhook |
| **A.8 Sécurité du développement** | Architecture en couches, revue de code (PR), SonarCloud, tests automatisés |
| **A.8 Gestion des vulnérabilités** | Registre §4, analyse statique, plan de scan des dépendances |
| **A.8 Journalisation / supervision** | Logs structurés, intercepteur de requêtes lentes, `/health` (voir [`50-Scalabilite-et-Performance.md`](50-Scalabilite-et-Performance.md)) |
| **A.8 Sécurité réseau** | CORS restrictif, BDD non exposée, conteneur non-root, HTTPS forcé |
| **A.5/A.8 Relations fournisseurs** | Sous-traitants identifiés (§2.4), secrets isolés par environnement |
| **A.8 Sauvegarde** | Volume Postgres persistant ; politique de sauvegarde à formaliser côté Cyna-Infra |

---

## 4. 🚨 Registre des vulnérabilités identifiées & plan de remédiation

Tenue d'un registre **honnête** des écarts repérés pendant la documentation. Chaque entrée porte une
sévérité, un impact et une action corrective.

| ID | Sévérité | Vulnérabilité | Impact | Remédiation | Réf. |
|---|---|---|---|---|---|
| SEC-01 | 🔴 Critique | `[Authorize]` **commenté** sur `DashboardController` | Statistiques entreprise (CA, users) accessibles **sans auth** | Réactiver `[Authorize(Roles=…)]` avec les bons libellés + test de non-régression | [`07-Dashboard-Statistiques.md`](07-Dashboard-Statistiques.md) |
| SEC-02 | 🔴 Critique | Chemin `/orders` (legacy) marque `Order=Paid` **sans vérifier le paiement** | Risque de fraude (commande non payée comptée comme payée) | Faire transiter tout paiement par le flux Stripe webhook ; déprécier le chemin mock | [`05-Panier-Commandes.md`](05-Panier-Commandes.md) |
| SEC-03 | 🟠 Élevé | Cookies `Secure=false` | Token transmissible en clair si HTTP | Passer `Secure=true` derrière HTTPS en prod (déjà HTTPS forcé) | [`01-Authentification-JWT-2FA.md`](01-Authentification-JWT-2FA.md) |
| SEC-04 | 🟠 Élevé | `[Authorize(Roles="Admin")]` (nom C#) ≠ claim `role` (libellé `[Description]`) | Routes mal protégées ou admins légitimes bloqués | Uniformiser sur la policy `AdminOnly` + test d'intégration | [`06-Categories.md`](06-Categories.md) |
| SEC-05 | 🟠 Élevé | Endpoint de debug `GET /debug-claims` `[AllowAnonymous]` | Fuite des claims du contexte | Exclure de la build prod (garde `IsDevelopment()`) ou supprimer | [`01-Authentification-JWT-2FA.md`](01-Authentification-JWT-2FA.md) |
| SEC-06 | 🟡 Moyen | Pas de **rate limiting** sur `/auth/*` (login, forgot-password, register) | Brute-force / spam d'emails | Middleware de rate limiting ASP.NET Core ou règle reverse proxy | [`02-Email-OTP.md`](02-Email-OTP.md) |
| SEC-07 | 🟡 Moyen | `?mock=true` du dashboard accessible **en prod** | Données factices servies en prod | Garde d'environnement (`IsDevelopment()`) | [`07-Dashboard-Statistiques.md`](07-Dashboard-Statistiques.md) |
| SEC-08 | 🟡 Moyen | Pas de purge des OTP expirés/utilisés | Croissance illimitée des tables de codes | Job de nettoyage planifié | [`02-Email-OTP.md`](02-Email-OTP.md) |
| SEC-09 | 🟢 Mineur | Politique de mot de passe incohérente (6 vs 8) | Mots de passe faibles à l'inscription | Politique unique (longueur + complexité) | [`02-Email-OTP.md`](02-Email-OTP.md) |
| SEC-10 | 🟢 Mineur | Révocation de token non instantanée (désactivation bloque le refresh, pas l'access en cours 15 min) | Fenêtre résiduelle de 15 min | Réduire la durée de l'access token ou blocklist | [`03-Gestion-Utilisateurs.md`](03-Gestion-Utilisateurs.md) |

> Ces correctifs sont priorisés dans la roadmap : [`70-Gouvernance-et-Roadmap.md`](70-Gouvernance-et-Roadmap.md).
> Les vérifications correspondantes figurent dans le plan de tests de sécurité :
> [`30-Strategie-et-Resultats-de-Tests.md`](30-Strategie-et-Resultats-de-Tests.md) §6.

---

## 🔗 Documents liés

* [`01-Authentification-JWT-2FA.md`](01-Authentification-JWT-2FA.md)
* [`02-Email-OTP.md`](02-Email-OTP.md)
* [`PAIEMENT-STRIPE-CONFIG.md`](PAIEMENT-STRIPE-CONFIG.md) — secrets & sécurité paiement
* [`30-Strategie-et-Resultats-de-Tests.md`](30-Strategie-et-Resultats-de-Tests.md)
* [`70-Gouvernance-et-Roadmap.md`](70-Gouvernance-et-Roadmap.md)
