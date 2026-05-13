# Feature Reorganization Plan

Updated: 2026-05-13

## Plan

1. Pull latest `origin/main`.
2. Read the requested navigation, Settings, Customers, Orders, GraphQL, plugin, license, LLM, settings, bootstrap, and README files.
3. Add customer search to repository/viewmodel/UI without loading profiles for every row.
4. Move Customer Loyalty out of Settings and into Customers.
5. Move GraphQL out of Settings into a Main Menu page.
6. Move Plugins out of Settings into a Main Menu page and avoid plugin scanning at startup.
7. Keep LLM configuration in Settings, make Reports usage clear, and add a test command.
8. Make License activation guidance explicit in Settings and README.
9. Keep startup lightweight by not persisting Reports, GraphQL, or Plugins as startup pages.
10. Update README and validation notes.
11. Run restore/build, commit, and push `origin/main`.

## Build Commands

```powershell
dotnet restore .\ProjectTest.csproj
dotnet build .\ProjectTest.csproj -c Debug -p:Platform=x64
```

## Manual Validation Flow

1. Open app and login with `admin / MyShop123!`.
2. Confirm Main Menu has Customers, GraphQL, and Plugins.
3. Customers: search by name and phone, clear search, add/edit/delete a customer without orders, select customer with orders and verify loyalty/history.
4. Settings: confirm only General, Credentials, License, Backup / Restore, and LLM / SLM Assistant remain.
5. GraphQL: Load Sample, Execute, verify JSON output and JSON error for bad query.
6. Plugins: open page, refresh, verify plugin status without crash.
7. Reports: verify Assistant not-configured message without key; after Settings key/endpoint save, Reports uses Assistant.
8. Switch between Dashboard, Products, Orders, Customers, Reports, GraphQL, Plugins, and Settings to check responsiveness.

## Known Follow-Up

- Orders still uses a customer ComboBox. A searchable customer selector can be added later if demo data grows beyond a comfortable dropdown size.
