# Developer Notes

## Purpose
This file collects practical notes for building, rebuilding the database, and refreshing the gaming accessories seed dataset.

## Build
```powershell
dotnet build ProjectTest.csproj -c Debug -p:Platform=x64
```

## Publish
```powershell
dotnet publish ProjectTest.csproj -c Debug -p:Platform=x64 -p:GenerateAppxPackageOnBuild=true -p:AppxPackageDir=AppPackages\ -p:AppxBundle=Never
```

## Database Defaults
- Environment variable override: `MYSHOP_CONNECTION_STRING`
- Default fallback database: `myshop_gaming_accessories`
- Rebuild helper:
```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\rebuild-dev-db.ps1
```

## Seed Dataset Refresh
Use the Python refresh script to rebuild the local dataset JSON, packaged product images, and the sample products document.

```powershell
python .\scripts\build_gaming_accessory_seed_assets.py
```

PowerShell wrapper:
```powershell
powershell -ExecutionPolicy Bypass -File .\download_gaming_accessory_images.ps1
```

Options:
- `--dataset-only`
  refreshes `gaming_accessories_seed_data.json` and `docs/sample_products.md`
- `--images-only`
  rebuilds `Assets/GamingProducts` from the existing dataset JSON

## Image Assets
- Packaged folder: `Assets/GamingProducts`
- Public image source: Unsplash
- Local source-image cache: `Assets/_source_gaming_images`

## Excel Import
The `.xlsx` import still expects the current schema headers:
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

Those legacy spec columns now represent five generic accessory spec lines.
