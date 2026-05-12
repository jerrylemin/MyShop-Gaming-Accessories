# Startup Freeze Audit

Updated: 2026-05-13

## Actual Fault

- The app could restore `LastOpenedScreen=Reports` during `MainWindow` startup.
- `ReportsPage` immediately started report loading after navigation.
- Core report loading, ML insights, and assistant analysis were all part of one snapshot path.
- `NavigationService.Navigate` synchronously waited on `SettingsService.SaveAsync(...).GetAwaiter().GetResult()` on the UI path.
- The Reports progress ring could continue animating while input was effectively delayed by startup report work.

## Startup Findings

- `Views/MainWindow.xaml.cs` now normalizes startup restore to lightweight screens only: `Dashboard`, `Products`, `Orders`, and `Settings`.
- `Reports` and `ProductEdit` are not restored on startup; they fall back to `Dashboard`.
- `Services/NavigationService.cs` now persists only lightweight startup screens.
- Navigating to `Reports` after the app is open still works, but it is not saved as the next startup target.

## Navigation Findings

- `NavigationService.Navigate` no longer blocks on settings persistence.
- Settings save is fire-and-forget behind `SaveSettingsSafelyAsync`; persistence failures do not block navigation.
- Forced `UpdateLayout()` calls were removed from `MainWindow.RefreshContentLayout` to avoid synchronous layout stalls after page navigation.

## Dialog Check

- License and onboarding dialogs still run as visible `ContentDialog` instances after navigation initialization.
- No hidden dialog or overlay was added.
- The startup freeze fix does not depend on suppressing trial or onboarding behavior.
