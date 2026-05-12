# MyShop Full-10 Progress

Updated: 2026-05-12

## Completed

- Pulled `origin main`; repository was already up to date.
- Read required project files and folders at audit level:
  - `README.md`
  - `ProjectTest.csproj`
  - `ProjectTest.slnx`
  - `App.xaml.cs`
  - `Services`
  - `ViewModels`
  - `Views`
  - `Repositories`
  - `Models`
  - `DataAccess`
  - `tools`
  - `ProjectTest.Tests`
- Created root handoff docs required for future Codex sessions.

## Completed

- Implemented invoice PDF export and FileSavePicker flow.

## In Progress

- Implement GraphQL schema/executor and Settings demo UI.

## Remaining

- GraphQL schema/executor and Settings demo.
- Microsoft.ML pipeline and Reports display improvements.
- Real LLM HTTP integration.
- Obfuscator config/script.
- UI automation demo script.
- Test target alignment and additional logic tests.
- Sample plugin project.
- Role/login/logout hardening.
- Customer loyalty UI.
- UI polish and final validation.

## Commands To Run

```powershell
dotnet restore ProjectTest.csproj
dotnet build ProjectTest.csproj -c Debug -p:Platform=x64
dotnet test ProjectTest.slnx
```

Optional if local PostgreSQL is available:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\rebuild-dev-db.ps1
dotnet run --project .\tools\VerificationRunner\VerificationRunner.csproj
```
