# Reports Lag Fix Plan

Updated: 2026-05-13

## Goals

- The app must open to an immediately clickable page even if the previous session ended in Reports.
- Reports must load only when the user explicitly navigates to Reports after startup.
- Leaving Reports while it loads must cancel the active work and prevent stale results from updating UI.
- Core report data must arrive before slower ML and assistant work.

## Implemented Changes

- `Views/Pages/ReportsPage.xaml.cs`
  - Creates a navigation-scoped `CancellationTokenSource`.
  - Defers initial load with `DispatcherQueuePriority.Low`.
  - Cancels active report work in `OnNavigatedFrom`.

- `ViewModels/ReportsViewModel.cs`
  - `InitializeAsync` and `LoadAsync` accept `CancellationToken`.
  - Refresh cancels any previous active report or insight request.
  - A load version guard prevents old requests from updating UI after cancellation or navigation.
  - Core snapshot updates the UI first.
  - ML and assistant insights load afterward without keeping the app navigation blocked.

- `Services/ReportingService.cs`
  - Adds `GetCoreSnapshotAsync` for fast report data.
  - Adds `GetReportInsightsAsync` for ML/assistant work.
  - Keeps `GetSnapshotAsync` for existing callers such as GraphQL.
  - Uses `AsNoTracking()` and passes cancellation tokens into EF Core async queries.
  - Keeps line chart data to at most 90 points, top products to 8, and pie share to 6.
  - Uses weekly or monthly line buckets when the selected date range is long.

- `Services/MlInsightService.cs` and `Services/LlmAssistantService.cs`
  - Accept cancellation tokens.
  - Use no-tracking queries where applicable.
  - Move ML forecast fitting off the UI continuation path.

- Chart controls
  - `SimpleLineChart` downsamples to 90 points, skips dense markers, and limits labels.
  - `SimpleBarChart` renders at most 12 rows.
  - `SimplePieChart` renders at most 6 slices.
  - Empty or null item sources remain safe.

## Non-Goals

- No installer or setup executable changes.
- No database schema changes.
- Reports functionality was kept and made cancellable/deferred instead of removed.
