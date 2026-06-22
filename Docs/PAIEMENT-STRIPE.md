# 💳 Paiement Stripe — Cyna API

> Mémo de reprise. Résume ce qui est fait, comment ça marche, et ce qu'il reste à faire.
> Dernière mise à jour : implémentation Phases 0 → 3 (backend).

---

## 1. Où on en est

| Phase | Contenu | État |
|-------|---------|------|
| **0** | Fondation : package `Stripe.net`, config, abstraction `IPaymentService`, bascule Mock/Stripe, migration `User.StripeCustomerId` | ✅ Fait |
| **1** | `StripePaymentService` réel : Customer + Subscription (`price_data` inline) + endpoint `POST /payments/subscription` | ✅ Fait |
| **2** | Webhook `POST /payments/webhook` (source de vérité) + commande créée en `Pending` à l'init | ✅ Fait |
| **3** | Tests en mode test Stripe | 🟡 **En cours** (init validé en réel, boucle webhook à finir par toi) |
| **4** | Front : remplacer le formulaire CB par Stripe Elements | ⬜ À faire (hors backend) |

### Déjà validé EN RÉEL (contre ton compte Stripe test)
- ✅ `POST /payments/subscription` → **201** avec de vrais ids Stripe
- ✅ Client Stripe créé (`cus_…`), Abonnement créé (`sub_…`), client secret réel (`pi_…_secret_…`)
- ✅ Commande locale créée en base en statut **Pending** (total correct, bons articles)
- ✅ Webhook rejette une signature invalide (**400**)

### Reste à valider toi-même
- 🟡 La **boucle webhook complète** : payer → `invoice.paid` → commande passe **Paid** + facture + panier vidé.
  (Nécessite `stripe login` dans ton navigateur, que je ne peux pas faire à ta place.)

---

## 2. Le flux de paiement (schéma)

```
   FRONT (navigateur)                 BACKEND (Cyna API)                 STRIPE
   ─────────────────                  ──────────────────                 ──────

  1. POST /payments/subscription
     (adresse de facturation)  ─────────────►
                                  • recalcule le panier (prix serveur)
                                  • crée Order(Pending) + Address en base
                                  • crée 1 Subscription Stripe / ligne ──────► crée Customer + Subscription
                                                                       ◄────── renvoie client_secret (1ère facture)
                                  • crée Subscription(s) locale(s) Pending
                            ◄─────  renvoie { orderId, clientSecret, publishableKey }

  2. Stripe Elements confirme
     le paiement (carte)       ──────────────────────────────────────────────► débite la carte
                                                                                 │
                                                                                 ▼
  3.                              POST /payments/webhook  ◄───────────────── invoice.paid (signé)
                                  • vérifie la signature
                                  • Subscription → Active
                                  • Order → Paid
                                  • crée la Facture
                                  • vide le panier
                            ◄─────  200 OK

  4. GET /user/orders → status "Paid" ✅
```

**Principe clé : le webhook est la _source de vérité_.** Tant que Stripe n'a pas confirmé le
paiement (via le webhook), la commande reste `Pending`. On ne fait jamais confiance au front
pour dire « c'est payé ». Robuste même si l'utilisateur ferme l'onglet.

---

## 3. Architecture (couches + fichiers clés)

```
Domain/Dto/Payments/        → DTOs (requêtes/réponses paiement)
Infrastructure/Interfaces/IPaymentService.cs      → contrat de la passerelle
Infrastructure/Payments/
   ├── PaymentOptions.cs          → options (Provider, Currency, clés Stripe)
   ├── MockPaymentService.cs      → fausse passerelle (défaut, pour dev/tests)
   └── StripePaymentService.cs    → vraie passerelle Stripe
Application/Services/
   ├── CheckoutService.cs         → init : recalcul panier + Order Pending + appel passerelle
   └── PaymentWebhookService.cs   → traitement des events Stripe (confirme tout)
Api/Controllers/PaymentController.cs  → POST /payments/subscription  +  POST /payments/webhook
```

**Bascule Mock ↔ Stripe** : pilotée par `Payments:Provider` dans la config.
- `"Mock"` → aucune connexion réseau, faux ids (pour tests/CI).
- `"Stripe"` → vrais appels Stripe.

---

## 4. Ce qu'il reste à faire

### A. Finir le test de la boucle webhook (tâche #11)

> Objectif : prouver que `invoice.paid` fait bien passer la commande en `Paid`.

**Étape 1 — secret webhook** (terminal 1, à laisser ouvert) :
```
stripe login
stripe listen --forward-to http://localhost:5104/payments/webhook
```
→ copie le `whsec_…` affiché, colle-le dans `Api/appsettings.Development.json` → `Stripe:WebhookSecret`.

**Étape 2 — démarrer l'API** (terminal 2) :
```
dotnet run --project Api
```

**Étape 3 — créer un paiement à confirmer** (terminal 3) :
```
curl -s -c j.txt -X POST http://localhost:5104/auth/login -H "Content-Type: application/json" -d "{\"email\":\"teststripe@cyna.fr\",\"password\":\"Test123!\"}"
curl -s -b j.txt -X POST http://localhost:5104/cart -H "Content-Type: application/json" -d "{\"pricingPlanId\":1,\"quantityUsers\":2,\"quantityDevices\":1}"
curl -s -b j.txt -X POST http://localhost:5104/payments/subscription -H "Content-Type: application/json" -d "{\"address\":{\"firstName\":\"Test\",\"lastName\":\"Stripe\",\"line1\":\"12 rue de la Paix\",\"postalCode\":\"75001\",\"city\":\"Paris\",\"country\":\"FR\"}}"
```
→ note le `pi_…` (partie AVANT `_secret_` dans `clientSecret`).

**Étape 4 — payer avec une carte de test** :
```
stripe payment_intents confirm pi_XXXXXXXX --payment-method pm_card_visa
```

**Étape 5 — vérifier** :
```
curl -s -b j.txt http://localhost:5104/user/orders
```
→ attendu : `status: "Paid"` + `invoiceUrl` renseigné. 🎯
(Dans le terminal 1, tu verras `invoice.paid → 200`.)

Cartes de test : `pm_card_visa` (succès), `pm_card_chargeDeclined` (refus), `pm_card_authenticationRequired` (3DS).

### B. Phase 4 — Front Stripe Elements (tâche #12, hors backend)

Dans `Cyna-Web/src/pages/checkout.jsx` :
1. Installer `@stripe/stripe-js` + `@stripe/react-stripe-js`.
2. Supprimer le formulaire CB brut + le faux `pi_mock_`.
3. Appeler `POST /payments/subscription` (avec l'adresse) → récupérer `clientSecret` + `publishableKey`.
4. Monter `<Elements>` + `<PaymentElement>`, confirmer avec `stripe.confirmPayment(...)`.
5. Gérer 3DS + la page de confirmation après retour.

---

## 5. Rappels / pièges à connaître

- **Clés Stripe** : uniquement dans `Api/appsettings.Development.json` (gitignoré).
  Jamais dans `appsettings.json` (versionné). Le `.example` ne contient que des placeholders.
- **Base locale** : si l'API refuse de démarrer (« table already exists »), supprime
  `Api/CynaApi.db` (+ `-shm`/`-wal`) — elle sera recréée. Ajoute `-- --seed` pour re-remplir.
- **Migrations EF** : toujours les générer avec le provider **Postgres**, jamais le défaut sqlite :
  `DatabaseProvider=postgres ConnectionStrings__DefaultConnection="Host=localhost;Database=cyna;Username=postgres;Password=postgres" dotnet ef migrations add <Nom> -p Infrastructure -s Api`
- **Bascule Mock/Stripe** : `Payments:Provider` = `"Mock"` (défaut, sûr) ou `"Stripe"` (réel).
- **Rien n'est commité** : toutes les modifs Stripe sont en local (working tree). À committer quand tu veux.
- **Données de test Stripe** : les `cus_`/`sub_` créés sont visibles dans ton dashboard (mode Test). Tu peux les supprimer à tout moment.

---

## 6. Cheat sheet

| Besoin | Commande |
|--------|----------|
| Démarrer l'API | `dotnet run --project Api` |
| Démarrer + re-seed | `dotnet run --project Api -- --seed` |
| Build complet | `dotnet build CynaApi.sln` |
| Écouter les webhooks | `stripe listen --forward-to http://localhost:5104/payments/webhook` |
| Payer un PI de test | `stripe payment_intents confirm pi_XXX --payment-method pm_card_visa` |
| User de test | `teststripe@cyna.fr` / `Test123!` |
| Plan mensuel de test | `pricingPlanId = 1` (produit 1) |
| API locale | http://localhost:5104 (Swagger/Scalar sur `/`) |

---

*Bonne nuit ! Demain : finir l'étape 4 du test webhook (§4.A), puis attaquer le front (§4.B).*
