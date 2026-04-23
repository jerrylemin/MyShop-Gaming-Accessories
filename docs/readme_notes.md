# README Regeneration Notes

## Files Scanned

The scan covered all repository `.cs`, `.xaml`, `.csproj`, `.json`, `.ps1`, and `.md` files outside generated build folders such as `bin/`, `obj/`, and `.vs/`.

Primary groups scanned:

- Root/config files:
  - `README.md`
  - `ProjectTest.csproj`
  - `App.xaml`
  - `App.xaml.cs`
  - `dotnet-tools.json`
  - `Properties/launchSettings.json`
- Existing documentation:
  - all markdown files in `docs/`
  - `.ai_memory/DATABASE_MAP.md`
  - `.ai_memory/PROJECT_IDENTITY.md`
  - `tmp/docs/project_proposal_submission_with_cover.md`
- Data/database layer:
  - all files in `DataAccess/`
  - all files in `Repositories/`
  - all files in `Models/`
- UI/application layer:
  - all files in `Views/`
  - all files in `ViewModels/`
  - all files in `Controls/`
  - all files in `Helpers/`
- Automation/tooling:
  - `download_gaming_accessory_images.ps1`
  - `scripts/rebuild-dev-db.ps1`
  - `scripts/build_gaming_accessory_seed_assets.py`
  - `tools/DatabaseRebuilder/*`
  - `tools/VerificationRunner/*`
  - `tools/generate_proposal_docx.ps1`
  - `tools/generate_proposal_docx.py`

## Main Features Documented

- Local login and saved credentials
- Database setup fallback window
- Dashboard KPIs and charts
- Product listing, detail view, add/edit/delete
- Search, filter, sort, and paging
- Excel import
- Order history and inline order editing
- Stock synchronization
- Revenue and product sales reports
- Settings for paging and credential cleanup
- Seed data generation and packaged images
- Database rebuild and verification tooling

## Assumptions Made

- `MyShop Gaming Accessories POS` is the real current project title because it is consistently used in code, XAML titles, existing docs, and proposal files.
- The repository should be documented in English because the technical documentation is mostly English even though proposal-oriented files are Vietnamese.
- `bin/`, `obj/`, and `.vs/` were treated as generated output and excluded from the documentation scan summary.
- The documented default PostgreSQL connection string and bootstrap login are development defaults, not production guidance.

## Sections Added or Rewritten

- `README.md` was fully rewritten.
- `docs/project_overview.md` was created.
- `docs/developer_notes.md` was rewritten for the current codebase.
- `docs/readme_notes.md` was created.
- `.ai_memory/PROJECT_IDENTITY.md` was refreshed.
- `.ai_memory/CHANGE_LOG.md` was created.
