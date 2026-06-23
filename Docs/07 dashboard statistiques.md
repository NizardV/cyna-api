# Dashboard & Statistiques Admin — Cyna API

## 🎯 Objectif du document

Détailler le fonctionnement du `DashboardController`, le système de filtrage temporel partagé, et le **mode mock** (génération de données factices via Bogus) mis en place tant que le module de paiement n'est pas finalisé.

---

## ⚠️ 0. Statut temporaire important

> Toutes les routes du dashboard acceptent un paramètre **`?mock=true`**, documenté comme **temporaire** dans le code (`RevenueStatsDto`, `OrderStatsDto`, `SubscriptionStatsDto`, `TopProductDto`) :
>
> *« Ce mode existe temporairement car l'intégration des paiements et la collecte des données métier sont développées en parallèle et les données réelles ne sont pas encore disponibles dans tous les environnements. »*

De plus, la protection d'accès est **désactivée dans le code** :

```csharp
// [Authorize(Roles = "Admin,SuperAdmin")]
```

🚨 **Le contrôleur entier est actuellement accessible sans authentification.** C'est un point bloquant pour la mise en production — cette ligne doit être réactivée (et corrigée pour utiliser les bons libellés de rôles, voir `06-Categories.md` pour la même problématique de nommage) avant tout déploiement définitif.

---

## 📊 1. Filtrage temporel commun (`DashboardFilterDto`)

Toutes les routes acceptent les mêmes paramètres de filtre, résolus via `DashboardFilterDto.Resolve()` :

```csharp
public (DateTime Start, DateTime End) Resolve()
{
    if (From.HasValue && To.HasValue)
        return (From.Value.Date, To.Value.Date.AddDays(1));  // To inclusif

    return Period switch
    {
        Week  => (now.AddDays(-7), now),
        Month => (now.AddMonths(-1), now),
        Year  => (now.AddYears(-1), now),
        All   => (DateTime.MinValue, now),
        _     => (now.AddMonths(-1), now),
    };
}
```

* **Priorité** : si `From` ET `To` sont fournis, ils priment sur `Period`.
* `To` est rendu **inclusif** en ajoutant un jour à la date (bornes `[Start, End)` demi-ouvertes).
* `Period` par défaut : `Month`.

---

## 🛣️ 2. Routes disponibles

| Route | DTO de réponse | Spécificité |
|---|---|---|
| `GET /dashboard/ca` | `RevenueStatsDto` | CA total, période courante, période précédente, % de croissance, historique mensuel |
| `GET /dashboard/orders` | `OrderStatsDto` | Total, répartition par statut, historique mensuel |
| `GET /dashboard/users` | `UserStatsDto` | Total, nouveaux sur la période, emails vérifiés, historique mensuel |
| `GET /dashboard/subscriptions` | `SubscriptionStatsDto` | Total, actifs (indépendant de la période), répartition par statut, historique mensuel |
| `GET /dashboard/products/top` | `IEnumerable<TopProductDto>` | Top produits par `Revenue` ou `Orders`, `limit` paramétrable |

### Calcul du CA réel (`DashboardRepository.GetRevenueStatsAsync`)

Filtré strictement sur les commandes au statut **`Paid`** :

```csharp
var paidOrders = _context.Orders.Where(o => o.Status == OrderStatus.Paid);
```

Le taux de croissance gère le cas `previousPeriod == 0` :

```csharp
var growthPercent = previousPeriod == 0m
    ? (currentPeriod > 0m ? 100m : 0m)
    : Math.Round((currentPeriod - previousPeriod) / previousPeriod * 100m, 2);
```

→ évite une division par zéro ; si la période précédente est nulle et la période courante positive, la croissance est forfaitairement affichée à `100%`.

### Top produits réels (`DashboardRepository.GetTopProductsAsync`)

Basé sur les `OrderItems` des commandes `Paid` dans la période, groupés par `(ProductId, ProductNameSnapshot)` :

```csharp
Revenue     = g.Sum(oi => oi.UnitPriceUsers * oi.QuantityUsers + oi.UnitPriceDevices * oi.QuantityDevices)
OrdersCount = g.Select(oi => oi.OrderId).Distinct().Count()   // commandes distinctes, pas le nombre de lignes
```

Les images produit sont récupérées en **une requête séparée** (`Dictionary<int, string?>`) plutôt que par jointure SQL complexe — choix de simplicité/performance documenté dans le code.

---

## 🎲 3. Mode mock (Bogus)

### Graine fixe (`MockSeed = 2026`)

```csharp
var faker = new Faker { Random = new Randomizer(MockSeed) };
```

Une graine fixe garantit que **des appels successifs avec `?mock=true` sur la même période renvoient des montants cohérents entre les différentes routes** (CA, commandes, top produits) — essentiel pour une démonstration crédible où les chiffres ne se contredisent pas entre les widgets du dashboard.

### Données générées par route

| Route | Plage Bogus |
|---|---|
| CA mensuel | `Random.Decimal(2_000, 15_000)` par mois |
| Commandes mensuelles | `Random.Int(10, 80)` par mois |
| Utilisateurs mensuels | `Random.Int(5, 40)` par mois |
| Abonnements mensuels | `Random.Int(8, 50)` par mois |
| Top produits | 6 noms fixes (`MockProductNames`), CA `Random.Decimal(1_000, 25_000)`, commandes `Random.Int(5, 200)` |

### Répartition par statut (proportions fixes, codées en dur)

**Commandes** : `pending 10%`, `paid 65%`, `failed 5%`, `refunded 5%`, `cancelled 15%`.
**Abonnements** : `active 55%`, `cancelled 15%`, `expired 15%`, `suspended 5%`, `pending 10%`.

> Ces clés sont en minuscules, alignées explicitement sur le format attendu par le frontend (`OrderStatus`/`SubscriptionStatus` convertis en `ToLowerInvariant()`), **identique au format des données réelles** (`DashboardRepository`) — garantit l'interchangeabilité mock/réel sans adaptation frontend.

### `RangeHelper.GenerateMonthlyRange` — protection anti-explosion

```csharp
var safeStart = start < end.AddYears(-2) ? end.AddYears(-2) : start;
```

Si le filtre `All` ou une plage `From`/`To` très large est demandée, la génération de séries mensuelles est **plafonnée à 24 mois** — évite de générer des centaines de points de données factices si la période couvre, par exemple, 10 ans.

---

## ⚠️ 4. Points d'attention critiques

* 🚨 **`[Authorize]` est commenté** sur `DashboardController` — accès libre aux statistiques de l'entreprise (CA, utilisateurs, etc.) sans authentification. **À corriger avant toute mise en production.**
* Le paramètre `mock=true` reste accessible en production tel que codé actuellement — aucune garde d'environnement (`IsDevelopment()`) ne l'empêche. À restreindre si la route doit un jour passer en production avec des vraies données sensibles.
* Une fois le module de paiement stabilisé, le code suggère de retirer le paramètre `mock` ou de le conserver uniquement comme fallback de développement (commentaire `RevenueStatsDto`).

---

## 🔗 Documents liés

* `00-Architecture-Generale.md`
* `05-Panier-Commandes.md` (source des données réelles de CA/commandes)
* `03-Gestion-Utilisateurs.md` (source des statistiques utilisateurs)