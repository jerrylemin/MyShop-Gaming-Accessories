# Customer Finish Plan

Updated: 2026-05-13

## Plan

1. Pull latest `origin/main` and verify the working tree.
2. Read the requested app, customer, navigation, settings, GraphQL, product, order, and installer files.
3. Finish Customers navigation from the main menu by registering the page explicitly and removing stale handler code.
4. Confirm customer management supports list, selection, add, edit, delete-without-orders, loyalty, lifetime spend, order history, and purchased products.
5. Shorten mechanical product, order, and settings copy without removing status messages or existing functions.
6. Keep GraphQL execute responsive with a light sample query, page-size clamps, timeout, `Running...` feedback, and JSON error output.
7. Validate startup and lag-sensitive flows: Reports is not restored at startup, Reports avoids reloading unchanged ranges, navigation settings save asynchronously, Settings does not auto-run GraphQL, and Customers limits order history.
8. Check installer structure without rebuilding `setup.exe`.
9. Run restore/build, update validation notes, commit, and push `main`.

## Implementation Notes

- Customer navigation is handled through `NavigationButton_Click` and `NavigationService.Register("Customers", typeof(CustomersPage))`.
- `CustomerNavigationSupport.cs` was deleted because the partial handler was no longer referenced.
- Products and Orders header text was shortened to the requested wording.
- Settings GraphQL execute now reports progress before awaiting execution and catches any unexpected UI-level exception into JSON output.
