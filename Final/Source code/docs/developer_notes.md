# Developer Notes

## Build

```powershell
dotnet restore ProjectTest.csproj
dotnet tool restore
dotnet build ProjectTest.csproj -c Debug -p:Platform=x64
```

## Submission Setup

Run the one-file setup from the repository root:

```powershell
powershell -ExecutionPolicy Bypass -File .\setup.ps1
```

What it does:

- checks .NET 8 SDK, Windows App Runtime 1.8, and PostgreSQL
- configures the PostgreSQL role and database
- sets the user-level `MYSHOP_CONNECTION_STRING`
- runs migrations and seed data through `tools/DatabaseRebuilder`
- builds the WinUI app
- publishes a runnable copy under `submission/ProjectTest-win-x64`
- creates `Run-ProjectTest.cmd` in the published folder

Useful options:

```powershell
powershell -ExecutionPolicy Bypass -File .\setup.ps1 -InstallPrerequisites
powershell -ExecutionPolicy Bypass -File .\setup.ps1 -ResetDatabase
powershell -ExecutionPolicy Bypass -File .\setup.ps1 -RunAfterSetup
```

## Run Context

- Project type: WinUI 3 desktop app with MSIX tooling enabled
- Target framework: `net8.0-windows10.0.19041.0`
- Default database: `myshop_gaming_accessories`
- Default connection string source order:
  1. `MYSHOP_CONNECTION_STRING`
  2. saved local setting
  3. built-in fallback in `DatabaseOptionsProvider`

## Bootstrap Login

```text
Username: admin
Password: MyShop123!
```

Saved credentials are encrypted in local app settings after a successful login.

## Database

- Provider: PostgreSQL via Npgsql
- Automatic migrations and seeding run on startup
- Legacy PascalCase schemas are normalized in `DatabaseInitializer`
- Clean rebuild helper:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\rebuild-dev-db.ps1
```

## Seed Data

- Seed generator: `DataAccess/Seeding/GamingAccessorySeedGenerator.cs`
- Embedded dataset: `DataAccess/Seeding/gaming_accessories_seed_data.json`
- Sample product document: `docs/sample_products.md`
- Packaged gallery: `Assets/GamingProducts`
- Source-image cache: `Assets/_source_gaming_images`

## Seed Asset Refresh

```powershell
python .\scripts\build_gaming_accessory_seed_assets.py
```

Optional modes:

- `--dataset-only` refreshes the JSON dataset and sample markdown only
- `--images-only` rebuilds `Assets/GamingProducts` from the current dataset

PowerShell wrapper:

```powershell
powershell -ExecutionPolicy Bypass -File .\download_gaming_accessory_images.ps1
```

## Excel Import

The Excel import expects the following headers:

- `SKU`
- `Name`
- `Category`
- `Manufacturer`
- `CPU`
- `RAM`
- `Storage`
- `GPU`
- `Screen`
- `ImportPrice`
- `SalePrice`
- `Stock`
- `Description`
- `Image1`
- `Image2`
- `Image3`

The five legacy spec columns are used as generic accessory spec slots in the current domain.

## Packaging

Example MSIX-oriented publish command:

```powershell
dotnet publish ProjectTest.csproj -c Debug -p:Platform=x64 -p:GenerateAppxPackageOnBuild=true -p:AppxPackageDir=AppPackages\ -p:AppxBundle=Never
```

File-system publish profiles are available under `Properties/PublishProfiles`.

## Verification Tooling

- `tools/DatabaseRebuilder` runs initialization and prints table counts
- `tools/VerificationRunner` creates a paid order and checks stock/dashboard/report behavior
- `dotnet build ProjectTest.csproj -c Debug -p:Platform=x64` was rechecked successfully during README regeneration
