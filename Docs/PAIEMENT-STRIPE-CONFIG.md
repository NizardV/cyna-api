# 💳 Paiement Stripe — Configuration & Déploiement

> ← [Retour à la vue d'ensemble](PAIEMENT-STRIPE.md)

---

## ⚙️ Configuration & secrets

```jsonc
// appsettings.json (versionné — valeurs non secrètes / placeholders vides)
"Payments": { "Provider": "Mock", "Currency": "eur" },
"Stripe":   { "SecretKey": "", "PublishableKey": "", "WebhookSecret": "" }
```

| Clé | Où la mettre | Source |
|---|---|---|
| `Stripe:SecretKey` (`sk_test_…`) | `appsettings.Development.json` (gitignoré) / env var | Dashboard → API keys |
| `Stripe:PublishableKey` (`pk_test_…`) | idem | Dashboard → API keys |
| `Stripe:WebhookSecret` (`whsec_…`) | idem | `stripe listen` (local) ou Dashboard → Webhooks (serveur) |

> ⚠️ **Jamais** de clé dans `appsettings.json` (versionné). Les vraies clés vivent dans
> `appsettings.Development.json` (gitignoré), `appsettings.Staging/Production.json` (gitignorés)
> ou des variables d'environnement.

### Exemple complet `Api/appsettings.Development.json` (gitignoré)

Ce fichier **surcharge** `appsettings.json` : il ne contient que ce qui change. C'est ici que tu
mets **tes** vraies clés de test.

```jsonc
{
  "Payments": {
    "Provider": "Stripe",          // "Stripe" pour activer le vrai mode (sinon "Mock")
    "Currency": "eur"
  },
  "Stripe": {
    "SecretKey":      "sk_test_…", // Dashboard → Developers → API keys
    "PublishableKey": "pk_test_…", // idem
    "WebhookSecret":  "whsec_…"    // donné par `stripe listen` (voir section suivante)
  }
}
```

> Un modèle est versionné dans `appsettings.Development.json.example` (placeholders).
> Tant que `Provider` reste `"Mock"`, les clés Stripe ne sont pas utilisées.

**Bascule Mock ↔ Stripe** : `Payments:Provider`. Le DI (`AppServicesExtensions`) enregistre
l'implémentation correspondante ; `Mock` reste le défaut sûr.

---

## 🛠️ Stripe CLI — tunnel webhook en local

En local, Stripe ne peut pas joindre `localhost`. La **Stripe CLI** ouvre un tunnel qui relaie les
events Stripe vers ton endpoint **et** fournit le `whsec_` de signature.

### 1. Installer (une fois)

```
# Windows
scoop install stripe          # ou : winget install stripe.stripe-cli
# (sinon télécharger le .exe depuis https://docs.stripe.com/stripe-cli)
stripe version                 # vérifie l'installation
```

### 2. Se connecter (une fois — ouvre le navigateur)

```
stripe login
```

→ affiche un *pairing code* (ex. `smiles-valor-wonder-salute`), appuie sur **Entrée** pour ouvrir
le navigateur et valider. Message attendu :
```
> Done! The Stripe CLI is configured for … with account id acct_…
```

### 3. Lancer le tunnel (à laisser ouvert pendant tous les tests)

```
stripe listen --forward-to https://localhost:7169/payments/webhook
```

→ affiche :
```
> Ready! Your webhook signing secret is whsec_xxxxxxxxxxxx (^C to quit)
```

👉 **Ce `whsec_xxxxxxxxxxxx` est la valeur à coller dans `Stripe:WebhookSecret`**
(`appsettings.Development.json`), puis **relancer l'API** pour qu'elle le prenne en compte.

### 4. (optionnel) Payer un PaymentIntent de test

Depuis un 3ᵉ terminal :
```
stripe payment_intents confirm pi_XXX --payment-method pm_card_visa
```

### ⚠️ Pièges courants

* Tape **une seule commande à la fois**. Ne colle **pas** les lignes d'explication (celles qui
  commencent par `→` ou `#`) dans le terminal — `cmd` essaierait de les exécuter.
* Le `whsec_` de la CLI **change à chaque nouvelle session** `stripe login` (et expire après 90 j).
* Garde le terminal `stripe listen` **ouvert** : fermé = plus aucun event relayé.
* L'URL `--forward-to` doit pointer sur le port réel de l'API (`https://localhost:7169` ici).

---

## 🚀 Mise en production

* **Pas de `stripe listen`** en déployé : on enregistre l'endpoint dans le **Dashboard → Webhooks**
  avec l'URL publique HTTPS (`https://.../payments/webhook`) ; Stripe pousse directement les events.
* Chaque environnement (dev local, staging, prod) a **son propre endpoint + son propre `whsec_`**.
* Le `WebhookSecret` du serveur va en **variable d'environnement** (`Stripe__WebhookSecret`).
* L'endpoint doit être **public et en HTTPS** (Stripe refuse le HTTP).

---

## 🔒 Sécurité

* Signature du webhook **toujours vérifiée** (`whsec_`) → rejet `400` si invalide.
* Montants **recalculés côté serveur** — le front ne peut pas imposer un prix.
* Endpoint webhook `[AllowAnonymous]` mais protégé par la signature ; aucun autre endpoint de
  paiement n'est anonyme (hors routes de test, **dev only**, qui renvoient `404` en production).
* Clés secrètes hors du dépôt.
