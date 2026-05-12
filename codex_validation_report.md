# MyShop Full-10 Validation Report

Updated: 2026-05-12

## Validation Status

- Initial audit complete.
- Implementation and validation are in progress.

## Required Commands

```powershell
dotnet restore ProjectTest.csproj
dotnet build ProjectTest.csproj -c Debug -p:Platform=x64
dotnet test ProjectTest.slnx
```

## Optional PostgreSQL Runtime Validation

Run only when PostgreSQL is available locally:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\rebuild-dev-db.ps1
dotnet run --project .\tools\VerificationRunner\VerificationRunner.csproj
```

## Demo Instructions Draft

- Login: use `admin / MyShop123!`; also verify `moderator` and `sale`.
- Products: open Products, use search/filter/paging, check gaming accessory specs and image gallery.
- Orders: create an order, choose customer, apply promotion, mark Paid, export invoice.
- Reports: choose date range, refresh, inspect charts, commissions, ML insights, and assistant summary.
- Settings: edit page size, LLM endpoint/key, plugins, and GraphQL demo after implementation.

## Known Issues Before Fixes

- GraphQL is string-switch fake implementation.
- ML insight does not use Microsoft.ML.
- LLM does not call HTTP endpoint.
- Test project target is net10 instead of net8.
- Obfuscator and sample plugin project are missing.

## Invoice Export Validation

- Implemented PDF output in `Services/InvoiceExportService.cs`.
- Implemented `FileSavePicker` in Orders print/export command.
- Debug x64 build passed after this change.
- Demo: open Orders, select or create an order, click `Print / Export invoice`, choose a `.pdf` location, then open the saved PDF with the default PDF reader.
