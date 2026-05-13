# Installer Fix Plan

## Implemented plan

1. Keep the existing Inno Setup path instead of replacing the installer.
2. Add `installer/database/myshop_demo.dump` as the preferred demo database artifact.
3. Add `scripts/export-demo-database.ps1` to export from `MYSHOP_CONNECTION_STRING` or `myshop.database.json` using `pg_dump -Fc`.
4. Add `scripts/restore-demo-database.ps1` to create/prepare the target database, restore the dump with `pg_restore`, verify main tables, write `myshop.database.json`, and fallback to the C# initializer when dump/tools are missing.
5. Update `installer/install-bootstrap.ps1` to check/install runtimes, install or reuse PostgreSQL, call restore first, and fallback to seed.
6. Update `installer/setup.iss` to include the restore script and dump, create the required shortcuts, and fail setup when bootstrap fails.
7. Update README with the grading install flow and demo accounts.
8. Add `scripts/test-installer-local.ps1` for local silent installer validation.

## Build command

Primary build:

```powershell
powershell -ExecutionPolicy Bypass -File .\installer\build-installer.ps1
```

Because an elevated installer test process locked `installer/output/setup.exe` in this Codex environment, the final successful compile was run with:

```powershell
& "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe" /O"D:\MEGA\lap trinh windows\ProjectTest\installer\output-final" "D:\MEGA\lap trinh windows\ProjectTest\installer\setup.iss"
```

## Teacher install flow

1. Run `setup.exe`.
2. Wait for runtime/database/bootstrap checks.
3. Open `MyShop Gaming Accessories POS` from Desktop or Start Menu.
4. Login with `admin / MyShop123!`, `moderator / MyShop123!`, or `sale / MyShop123!`.
