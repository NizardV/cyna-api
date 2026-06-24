# Cyna API

Back-end / API REST de la plateforme **Cyna** (e-commerce de services de cybersécurité).
ASP.NET Core · .NET 10 · architecture en couches (Clean Architecture) · PostgreSQL · Docker · CI/CD.

| | |
|---|---|
| 📚 **Documentation technique complète (DAT/DCT)** | **[Docs/README.md](Docs/README.md)** ← point d'entrée |
| 🚀 Installation & contribution | [INSTALLATION.md](INSTALLATION.md) |
| 🧩 Front web | dépôt `Cyna-Web` |
| 📱 App mobile | dépôt `Cyna-App` |
| 🏗️ Infrastructure | dépôt `Cyna-Infra` |

---

## Démarrage rapide

```bash
# SQLite (défaut, zéro config)
dotnet run --project Api

# Tests
dotnet test UnitTests/UnitTests.csproj
```

Détails (PostgreSQL via Docker, migrations EF Core, seed) : [INSTALLATION.md](INSTALLATION.md).

## Structure de la solution

| Projet | Rôle |
|---|---|
| `Domain` | Entités EF Core & DTOs (cœur métier) |
| `Application` | Services métier + interfaces |
| `Infrastructure` | `AppDbContext`, repositories EF Core, sécurité JWT, paiement Stripe |
| `Api` | Contrôleurs REST, `Program.cs`, configuration, intercepteurs |
| `Tools` | Enums & helpers transverses (hash, OTP, TOTP, email, claims) |
| `UnitTests` / `Api.IntegrationTests` | Tests |

> Toute la documentation (architecture, modules, schémas, tests, sécurité/RGPD, performance,
> gouvernance) et le **mapping vers la grille d'évaluation BC3** sont dans **[Docs/README.md](Docs/README.md)**.
