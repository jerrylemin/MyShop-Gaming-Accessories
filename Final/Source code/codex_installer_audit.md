# Installer Audit

Date: 2026-05-13

## Current installer

- Installer technology: Inno Setup.
- Main source: `installer/setup.iss`.
- Build wrapper: `installer/build-installer.ps1`.
- Bootstrap script run by installer: `installer/install-bootstrap.ps1`.
- Database bootstrap executable source: `installer/database/MyShop.DatabaseBootstrapper.csproj` and `installer/database/Program.cs`.
- Final built setup file from the latest successful compile in this Codex run: `installer/output-final/setup.exe`.
- Previous working setup file was left in place: `installer/output/setup.exe`.

## What setup.exe installs

- Published WinUI app files from `installer/staging/app`, including `ProjectTest.exe`, `ProjectTest.dll`, `ProjectTest.deps.json`, `ProjectTest.runtimeconfig.json`, dependency DLLs, and `Assets/GamingProducts`.
- Database bootstrapper files under `{app}\installer\database`.
- Restore script under `{app}\scripts\restore-demo-database.ps1`.
- Demo dump under `{app}\installer\database\myshop_demo.dump` when the dump exists.
- Bundled prerequisites:
  - .NET 8 Desktop Runtime x64.
  - Windows App Runtime 1.8 x64.
  - PostgreSQL 18 Windows x64.
- Desktop shortcut and Start Menu shortcut named `MyShop Gaming Accessories POS`.

## Database source

- Preferred source after this change: PostgreSQL custom-format dump.
- Dump path: `installer/database/myshop_demo.dump`.
- Dump was created in this environment by `scripts/export-demo-database.ps1`.
- Fallback source: `Services/DatabaseInitializer.cs` and `DataAccess/Seeding/GamingAccessorySeedGenerator.cs`.

## Gaps found

- The previous installer seeded through the C# database bootstrapper only; it did not restore a PostgreSQL dump first.
- The previous bootstrap contained fixed installer database passwords. It now generates installer-time database passwords unless environment variables are provided.
- Existing PostgreSQL with an unknown admin password caused setup failure. The bootstrap now installs an isolated MyShop PostgreSQL service on a free port if `MYSHOP_POSTGRES_ADMIN_PASSWORD` is not set.
- Inno `[Run]` did not make setup.exe fail when bootstrap failed. The bootstrap is now launched from Inno `[Code]` and raises an installer error on nonzero exit.
- `scripts/test-installer-local.ps1` now checks the common Public Desktop shortcut location as well as the current user's Desktop.
