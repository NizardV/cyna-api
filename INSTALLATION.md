# Contribuer à Cyna API

## Prérequis

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

---

## Premier clone

Cloner les deux repos dans le même dossier parent :

```bash
git clone https://2028DI1P5G3@dev.azure.com/2028DI1P5G3/DIIAGE%202028%20DI1%20P5%20G3/_git/Cyna-Api
git clone https://2028DI1P5G3@dev.azure.com/2028DI1P5G3/DIIAGE%202028%20DI1%20P5%20G3/_git/Cyna-Infra
```

La structure attendue est :

```
<dossier-parent>/
├── Cyna-Api/
└── Cyna-Infra/
```

> Le docker-compose local utilise un chemin relatif `../Cyna-Infra/...` — les deux repos doivent être côte à côte.

Ensuite, crée ton fichier de config local :

```bash
cp Api/appsettings.Development.json.example Api/appsettings.Development.json
```

Ce fichier est gitignored — chaque dev a le sien. Il configure SQLite par défaut, tu peux le modifier pour switcher sur Postgres (voir section dédiée).

---

## Lancer le projet en local

### Option 1 — SQLite (défaut, zéro config)

Rien à faire, SQLite est utilisé par défaut en développement.

```bash
dotnet run --project Api
```

L'API démarre sur `https://localhost:7169` et crée automatiquement `CynaApi.db` à la racine.

> Si le certificat HTTPS n'est pas approuvé sur votre machine :
> ```bash
> dotnet dev-certs https --trust
> ```

---

### Option 2 — PostgreSQL (recommandé pour tester en conditions réelles)

**1. Lancer Postgres via Docker**

```bash
docker compose -f ../Cyna-Infra/docker-compose/docker-compose.localdb.yml up -d
```

N'oublie pas de lancer Docker Desktop sur Windows

**2. Configurer l'API**

Crée ou modifie `Api/appsettings.Development.json` (ce fichier est gitignored) :

```json
{
  "DatabaseProvider": "postgres",
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=cyna_dev;Username=postgres;Password=postgres"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}
```

**3. Lancer l'API**

```bash
dotnet run --project Api
```

Les migrations EF Core sont appliquées automatiquement au démarrage.

**4. Seed (optionnel)**

```bash
dotnet run --project Api -- --seed
```

---

### Arrêter Postgres

```bash
docker compose -f ../Cyna-Infra/docker-compose/docker-compose.localdb.yml down
```

Pour supprimer aussi les données :

```bash
docker compose -f ../Cyna-Infra/docker-compose/docker-compose.localdb.yml down -v
```

---

## Ajouter une migration EF Core

Les migrations sont générées avec **PostgreSQL comme provider de référence** afin de garantir les bons types en base (boolean, timestamp, etc.). SQLite étant permissif, il accepte ces types sans problème.

**1. Switcher sur Postgres dans `appsettings.Development.json`**

```json
{
  "DatabaseProvider": "postgres",
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=cyna_dev;Username=postgres;Password=postgres"
  }
}
```

**2. Générer la migration**

```bash
dotnet ef migrations add NomDeLaMigration --project Infrastructure --startup-project Api
```

**3. Vérifier que tout applique correctement**

```bash
dotnet ef database update --project Infrastructure --startup-project Api
```

---

## ⚠️ Règles pour le SQL brut dans les migrations

Ne jamais écrire de SQL sans guillemets doubles — Postgres est sensible à la casse :

```csharp
// ❌ Ne pas faire (fonctionne en SQLite, crashe en Postgres)
migrationBuilder.Sql("UPDATE PricingPlans SET Col = 1;");

// ✅ Faire
migrationBuilder.Sql("""UPDATE "PricingPlans" SET "Col" = 1;""");
```