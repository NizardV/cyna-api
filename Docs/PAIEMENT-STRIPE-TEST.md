# 💳 Paiement Stripe — Tests & Cheat Sheet

> ← [Retour à la vue d'ensemble](PAIEMENT-STRIPE.md)

---

## 🧪 Guide de test (mode test Stripe)

1. Renseigner `Api/appsettings.Development.json` avec `Provider = "Stripe"` + clés de test
   (voir [PAIEMENT-STRIPE-CONFIG.md](PAIEMENT-STRIPE-CONFIG.md) § Configuration).
2. Lancer `stripe listen` pour obtenir le `whsec_` et relayer les events
   (voir [PAIEMENT-STRIPE-CONFIG.md](PAIEMENT-STRIPE-CONFIG.md) § Stripe CLI).
3. Démarrer l'API et le front :

```bash
dotnet run --project Api --launch-profile https    # terminal 1
stripe listen --forward-to https://localhost:7169/payments/webhook  # terminal 2
npm run dev --prefix ../Cyna-Web                   # terminal 3
```

4. Test rapide via Swagger/Scalar — routes `payments/test/*` :

```json
{ "amountCents": 100, "paymentMethod": "pm_card_visa" }
```

5. Test complet via le front : se connecter → panier → `/checkout` → adresse →
   **Procéder au paiement** → carte de test → **Payer**.

---

## 💳 Cartes de test

### Via l'API (`paymentMethod`)

| `paymentMethod` | Résultat |
|---|---|
| `pm_card_visa` / `pm_card_mastercard` / `pm_card_amex` | ✅ `succeeded` |
| `pm_card_chargeDeclined` | ❌ `declined` (`generic_decline`) |
| `pm_card_chargeDeclinedInsufficientFunds` | ❌ `insufficient_funds` |
| `pm_card_authenticationRequired` | 🔐 `requires_action` (3DS) |

### Via le front (Stripe Elements — formulaire carte)

| Numéro | Résultat |
|---|---|
| `4242 4242 4242 4242` | ✅ Succès |
| `4000 0000 0000 9995` | ❌ Fonds insuffisants |
| `4000 0025 0000 3155` | 🔐 3DS obligatoire |

> Date d'expiration future + CVC quelconques (ex. `12/34`, `123`).

---

## 📋 Cheat sheet

| Besoin | Commande |
|---|---|
| Démarrer l'API (https) | `dotnet run --project Api --launch-profile https` |
| Démarrer + re-seed | `dotnet run --project Api -- --seed` |
| Build complet | `dotnet build CynaApi.sln` |
| Écouter les webhooks | `stripe listen --forward-to https://localhost:7169/payments/webhook` |
| Payer un PI de test | `stripe payment_intents confirm pi_XXX --payment-method pm_card_visa` |
| Migration EF (⚠️ provider Postgres) | `DatabaseProvider=postgres ConnectionStrings__DefaultConnection="Host=localhost;Database=cyna;Username=postgres;Password=postgres" dotnet ef migrations add <Nom> -p Infrastructure -s Api` |
| User de test | `teststripe@cyna.fr` / `Test123!` |
| Plan mensuel de test | `pricingPlanId = 1` |

---

## ⚠️ Limites connues & évolutions

| Sujet | État actuel | Évolution possible |
|---|---|---|
| Produits Stripe | Créés à la volée (dédup intra-requête) | Persister `Product.StripeProductId` |
| TVA | 20 % calculé par ligne | Stripe Tax |
| Panier multi-périodicités | Plusieurs `clientSecret` (front gère le 1er) | Confirmation séquentielle côté front |
| Commande `Pending` orpheline | Possible si l'appel Stripe échoue après création locale | Job de nettoyage des `Pending` expirés |
| Codes promo | Non appliqués au montant Stripe | Intégration `coupons` Stripe |
| Gestion abonnement | Pas d'annulation côté user | Portail client Stripe |
