# MyShop Full-10 Implementation Plan

Updated: 2026-05-12

## Work Groups

1. Invoice export
   - Replace `.txt` invoice writer with real PDF export.
   - Use WinUI FileSavePicker from Orders before calling export service.
   - Include app name, order metadata, customer, salesperson, line items, subtotal, discount, and final total.

2. GraphQL
   - Add a real GraphQL schema/executor package.
   - Expose products, orders, reports, saveProduct, and saveOrder through schema fields.
   - Add Settings demo UI: sample query, editable query, Execute button, result JSON.

3. ML.Net and LLM
   - Add Microsoft.ML.
   - Train a small forecasting/restock pipeline from orders/products when enough data exists.
   - Keep a clear fallback message when data is insufficient.
   - Replace local LLM summary with OpenAI-style HTTP call using Settings/env config, timeout, and safe error handling.

4. Roles, loyalty, and UI polish
   - Query database users when possible.
   - Preserve current user role on saved auto-login.
   - Improve logout handling for saved credentials.
   - Hide sale import prices.
   - Add customer loyalty list and demo path.
   - Fix Settings mojibake and responsive/cut-off issues found during review.

5. Obfuscation, plugin demo, and UI automation
   - Add Obfuscar config and Release script without changing installer.
   - Add sample plugin project under `plugins` or `samples`.
   - Add a local UI automation smoke script that is excluded from normal headless tests.

6. Functional tests and validation docs
   - Align test project with net8 Windows target.
   - Add/repair tests for discount, license, product query options, order status transitions, reporting aggregation, invoice export, GraphQL, ML/LLM safe paths.
   - Update README and all Codex handoff markdown.
   - Run restore, build, tests, and optional DB verification if PostgreSQL is available.

## Commit Plan

- `complete invoice export`
- `add graphql demo`
- `add ml and llm integrations`
- `improve roles loyalty and ui`
- `add release obfuscation and plugin sample`
- `add validation tests`
- `polish final demo docs`
