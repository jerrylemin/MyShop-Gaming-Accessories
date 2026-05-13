# Installer Validation

Date: 2026-05-13

## Commands run

- `powershell -ExecutionPolicy Bypass -Command "git pull origin main"`: passed, already up to date.
- `powershell -ExecutionPolicy Bypass -Command "$PSVersionTable"`: passed, PowerShell 5.1.26100.7627.
- `powershell -ExecutionPolicy Bypass -Command "dotnet --version"`: passed, 8.0.420.
- `powershell -ExecutionPolicy Bypass -Command "dotnet --list-sdks"`: passed, SDKs 8.0.420 and 10.0.300-preview.
- `powershell -ExecutionPolicy Bypass -Command "where.exe psql"`: failed on PATH, but scripts found PostgreSQL tools under Program Files.
- `powershell -ExecutionPolicy Bypass -Command "where.exe pg_dump"`: failed on PATH, but export script found `pg_dump.exe`.
- `powershell -ExecutionPolicy Bypass -Command "where.exe pg_restore"`: failed on PATH, but restore script can find PostgreSQL tools under Program Files.
- `powershell -ExecutionPolicy Bypass -Command "where.exe iscc"`: failed on PATH; `installer/build-installer.ps1` found Inno Setup at `%LOCALAPPDATA%\Programs\Inno Setup 6\ISCC.exe`.
- `powershell -ExecutionPolicy Bypass -Command "where.exe candle"`: failed; WiX is not used.
- `powershell -ExecutionPolicy Bypass -Command "where.exe light"`: failed; WiX is not used.
- `powershell -ExecutionPolicy Bypass -Command "Remove-Item -Path '.\bin','.\obj','.\AppPackages','.\BundleArtifacts','.\submission' -Recurse -Force -ErrorAction SilentlyContinue"`: first shell returned exit 1 with no output, rerun with try/exit wrapper passed.
- `powershell -ExecutionPolicy Bypass -Command "dotnet restore .\ProjectTest.csproj"`: passed.
- `powershell -ExecutionPolicy Bypass -Command "dotnet build .\ProjectTest.csproj -c Debug -p:Platform=x64"`: passed, 0 warnings, 0 errors.
- `powershell -ExecutionPolicy Bypass -Command "dotnet build .\ProjectTest.csproj -c Release -p:Platform=x64"`: passed, trim warnings only, 0 errors.
- `powershell -ExecutionPolicy Bypass -File .\scripts\export-demo-database.ps1`: first run failed due workspace path quoting; fixed script and rerun passed.
- `powershell -ExecutionPolicy Bypass -Command "Get-Item .\installer\database\myshop_demo.dump -ErrorAction SilentlyContinue | Select-Object FullName,Length,LastWriteTime"`: passed, dump length 36,679 bytes.
- `powershell -ExecutionPolicy Bypass -File .\installer\build-installer.ps1`: passed after fixes, produced `installer/output/setup.exe`; later one rerun failed because a test installer process locked that output.
- Inno direct compile with `/O...\installer\output-final`: passed and produced `installer/output-final/setup.exe`.
- `powershell -ExecutionPolicy Bypass -Command "Get-ChildItem -Path . -Recurse -Filter setup.exe | Select-Object FullName,Length,LastWriteTime"`: found setup.exe under installer output folders.
- `powershell -ExecutionPolicy Bypass -Command "dotnet run --project .\tools\VerificationRunner\VerificationRunner.csproj"`: first run failed due stale stub method signatures; fixed `tools/VerificationRunner/ServiceStubs.cs`; rerun passed.
- `powershell -ExecutionPolicy Bypass -File .\scripts\test-installer-local.ps1 -SetupPath .\installer\output\setup.exe -Silent`: first meaningful run found app exe and Public Desktop shortcut but bootstrap failed; this exposed PostgreSQL 16 vs dump PostgreSQL 18 incompatibility and a Program Files backup permission issue. Both were fixed.
- `powershell -ExecutionPolicy Bypass -File .\scripts\test-installer-local.ps1 -SetupPath .\installer\output\setup.exe -Silent`: later run timed out because the previous elevated Inno process remained alive and could not be killed by this shell. This is recorded as an environment/process cleanup limitation.

## Artifacts

- Demo dump: `installer/database/myshop_demo.dump`, 36,679 bytes.
- Final setup compiled after latest fixes: `installer/output-final/setup.exe`.
- Previous setup left in place: `installer/output/setup.exe`.

## VerificationRunner output

```text
CreatedOrderId=443
OrderItemCount=1
StockBefore=17
StockAfter=16
DashboardTotalProducts=110
DashboardLowStockProducts=41
DashboardTodayRevenue=149000.00
DashboardRecentOrders=3
ReportRevenueByDayPoints=8
ReportProfitByDayPoints=8
ReportRevenueByMonthBars=1
ReportProfitByMonthBars=1
ReportTotalProfit=330295262.0000
ReportTopProducts=8
```

## Remaining environment notes

- PATH in this Codex shell does not expose PostgreSQL tools or Inno Setup even though they exist under Program Files/local app data.
- An elevated Inno setup process from an installer validation attempt remained running and locked `installer/output/setup.exe`; final compile was therefore written to `installer/output-final/setup.exe`.
- The old setup output was not deleted.
