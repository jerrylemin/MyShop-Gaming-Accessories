# MyShop Gaming Accessories POS

## Overview

MyShop Gaming Accessories POS is a WinUI 3 desktop point-of-sale application for a Vietnamese gaming accessories store. It supports local sign-in, PostgreSQL database configuration, seeded sample data, product and order workflows, dashboard analytics, sales reporting, and a small set of maintenance tools for rebuilding and verifying the development database.

This repository is a coursework-style Windows programming project built around a single desktop client and a PostgreSQL database.

## Main Features

- Login with locally saved encrypted credentials and a bootstrap default account.
- Database setup window for saving a PostgreSQL connection string when startup cannot connect.
- Dashboard with total products, low-stock count, today order count, today revenue, monthly revenue trend, latest orders, and top-selling products.
- Product management with add, edit, delete, detail view, packaged image gallery, live category data, and full category CRUD.
- Product search, category filter, price filter, sort options, and paging.
- Excel import for upserting products and creating missing categories.
- Order management with date-range filtering, keyword search, sort options, full paging controls, inline order editing, item lines, status changes, and deletion.
- Stock synchronization when orders are created, updated, cancelled, or deleted.
- Reports for revenue and profit by day, week, month, and year.
- Product sales analytics for top-selling items and sales share.
- Settings for items-per-page and saved login cleanup.
- Seed data for 5 categories, 110 products, and 440 seeded orders.
- Packaged gaming accessory images stored inside the app assets.

## Technology Stack

- WinUI 3
- C#
- .NET 8
- Windows App SDK 1.8
- Entity Framework Core 8
- PostgreSQL via Npgsql
- MVVM
- Repository + service layers
- PowerShell automation
- Python utility scripts
- Open XML SDK for Excel import

## Project Structure

- `Views` contains windows and pages for login, database setup, shell navigation, dashboard, products, orders, reports, and settings.
- `ViewModels` contains MVVM state and commands for each screen.
- `Models` contains entities, DTOs, query options, chart models, and operation result wrappers.
- `Services` contains startup/bootstrap, authentication, settings, navigation, dashboard/reporting aggregation, database initialization, and Excel import logic.
- `Repositories` contains EF Core query and transactional write logic for categories, products, and orders.
- `DataAccess` contains `DbContext`, factories, migrations, and seed generation.
- `Controls` contains reusable custom WinUI controls for KPI cards and charts.
- `Helpers` contains MVVM base classes, command helpers, currency formatting, and image conversion helpers.
- `Assets` contains WinUI package assets, source image caches, and the packaged `GamingProducts` gallery.
- `scripts` contains developer automation such as database rebuild and seed asset generation.
- `tools` contains small console utilities for database rebuild verification and proposal document generation.

## Database

The application uses PostgreSQL with EF Core and a single `MyShopDbContext`.

Main entities:

- `Category`
- `Product`
- `Order`
- `OrderItem`

Relationships:

- One category has many products.
- One product can appear in many order items.
- One order has many order items.

Database behavior:

- EF Core migrations are applied automatically at startup.
- `DatabaseInitializer` also detects older PascalCase schemas and attempts an in-place normalization before running migrations.
- If the database is empty, the app seeds categories, products, image paths, and sample orders automatically.
- The default database name is `myshop_gaming_accessories`.

Important schema note:

- The product table still uses legacy columns named `CPU`, `RAM`, `Storage`, `GPU`, and `Screen`.
- In the current gaming accessories domain, those fields are intentionally reused as five generic accessory spec lines to avoid a breaking schema rewrite.

## Setup Instructions

### Prerequisites

- Windows 10/11 with WinUI 3 development support
- .NET 8 SDK
- PostgreSQL server
- Windows App SDK runtime / WinUI tooling suitable for building and launching WinUI 3 apps

### Restore dependencies

```powershell
dotnet restore ProjectTest.csproj
dotnet tool restore
```

### Configure the database

The app resolves its connection string in this order:

1. `MYSHOP_CONNECTION_STRING` environment variable
2. Saved value in local app settings
3. Built-in fallback:

```text
Host=localhost;Port=5432;Database=myshop_gaming_accessories;Username=postgres;Password=jelly;Include Error Detail=true
```

If startup cannot connect, the app opens a Database Setup window where you can save a new connection string and retry.

### Initialize or rebuild the database

Automatic path:

- Start the app with a valid PostgreSQL connection.
- EF Core migrations run automatically.
- Seed data is inserted automatically if the database has no categories.

Submission setup path:

```powershell
powershell -ExecutionPolicy Bypass -File .\setup.ps1
```

The setup file checks the required local tooling, configures PostgreSQL role/database access,
runs migrations and seed data, builds the app, publishes a runnable copy under `submission`,
and creates `Run-ProjectTest.cmd` inside the published folder. Use `-InstallPrerequisites`
if .NET 8 SDK, Windows App Runtime 1.8, or PostgreSQL need to be installed with `winget`.
Use `-ResetDatabase` only when a clean recreated database is required.

Clean rebuild path:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\rebuild-dev-db.ps1
```

The rebuild script:

- locates the local PostgreSQL Windows service
- repairs or creates the target role if needed
- recreates the database
- restores local .NET tools
- runs the `DatabaseRebuilder` tool
- prints row-count verification

### Default login

If no saved credentials exist yet, the bootstrap login is:

```text
Username: admin
Password: MyShop123!
```

After a successful login, the credentials are encrypted and stored in local app settings for future launches.

### Build the application

```powershell
dotnet build ProjectTest.csproj -c Debug -p:Platform=x64
```

The current repository was rechecked with this command and builds successfully.

### Run the application

Typical developer options:

- Run from Visual Studio using either the packaged or unpackaged profile in `Properties/launchSettings.json`.
- Or build from CLI and launch through your normal WinUI 3 workflow on a machine with the required runtime/tooling installed.

## Seed Data and Assets

Seed sources:

- `DataAccess/Seeding/gaming_accessories_seed_data.json`
- `DataAccess/Seeding/GamingAccessorySeedGenerator.cs`

Current seeded content:

- 5 categories
- 110 products
- 440 orders

Category distribution:

- 22 Gaming Keyboard products
- 22 Gaming Mouse products
- 22 Gaming Headset products
- 22 Mousepad products
- 22 Streaming Gear products

Image assets:

- Packaged product gallery: `Assets/GamingProducts`
- Source image cache: `Assets/_source_product_images`
- Current packaged image count: 330 images
- Current source-image cache count: 330 images

The seed dataset stores Vietnamese-market retail pricing gathered from Phong Vu, and each product now keeps three product-specific images sourced from its own gallery before being copied into app asset paths like:

```text
ms-appx:///Assets/GamingProducts/{productId}_{imageNumber}.jpg
```

## Scripts and Tools

### Scripts

- `scripts/rebuild-dev-db.ps1`
  Recreates the PostgreSQL development database, runs migrations and seeding, and verifies row counts.
- `setup.ps1`
  One-file submission setup for checking prerequisites, preparing PostgreSQL, seeding the database,
  building/publishing the app, and generating a launcher in `submission`.
- `scripts/build_gaming_accessory_seed_assets.py`
  Refreshes the seed dataset, sample markdown document, and packaged images.
- `download_gaming_accessory_images.ps1`
  PowerShell wrapper around the Python seed asset builder.

Useful options:

```powershell
python .\scripts\build_gaming_accessory_seed_assets.py --dataset-only
python .\scripts\build_gaming_accessory_seed_assets.py --images-only
```

### Tools

- `tools/DatabaseRebuilder`
  Console app that runs `DatabaseInitializer` and prints category/product/order counts.
- `tools/VerificationRunner`
  Console app that creates a paid order and verifies stock, dashboard, and report behavior against a real database.
- `tools/generate_proposal_docx.ps1`
  Generates the submission `.docx` file from the markdown proposal.

## Build and Packaging

The project is configured as a WinUI 3 app with MSIX tooling enabled.

Relevant packaging/build facts:

- Target framework: `net8.0-windows10.0.19041.0`
- Supported platforms: `x86`, `x64`, `ARM64`
- Runtime identifiers: `win-x86`, `win-x64`, `win-arm64`
- Publish profiles exist for `win-x86`, `win-x64`, and `win-arm64`
- `Package.appxmanifest` is present
- `launchSettings.json` includes packaged and unpackaged launch profiles

Build:

```powershell
dotnet build ProjectTest.csproj -c Debug -p:Platform=x64
```

Example package/publish command:

```powershell
dotnet publish ProjectTest.csproj -c Debug -p:Platform=x64 -p:GenerateAppxPackageOnBuild=true -p:AppxPackageDir=AppPackages\ -p:AppxBundle=Never
```

Example file-system publish profile usage:

```powershell
dotnet publish ProjectTest.csproj -c Release -p:Platform=x64 -p:PublishProfile=win-x64.pubxml
```

## Known Issues or Notes

- The application requires a reachable PostgreSQL instance; otherwise startup falls back to the Database Setup window.
- The development defaults in scripts assume a local PostgreSQL server and the `postgres` user password `jelly`.
- The login system is local-only and does not implement roles, permissions, or external identity providers.
- Product spec fields are mapped onto legacy schema columns (`CPU`, `RAM`, `Storage`, `GPU`, `Screen`) for compatibility.
- Paid and cancelled orders are intentionally read-only in the editor.
- The shell is built programmatically in `Views/MainWindow.xaml.cs`; `Views/MainWindow.xaml` is only a minimal host.
- The app currently forces a light theme and does not expose a theme switcher.
- If you need signed MSIX distribution, add your own signing configuration outside the current repository state.
