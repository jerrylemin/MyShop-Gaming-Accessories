# Project Overview

## Purpose

MyShop Gaming Accessories POS is a WinUI 3 desktop POS application for managing gaming accessory inventory, orders, dashboard KPIs, and sales reports against a PostgreSQL database.

## Current Domain

- Vietnamese gaming accessories retail
- Core catalog categories:
  - Gaming Keyboard
  - Gaming Mouse
  - Gaming Headset
  - Mousepad
  - Streaming Gear

## Core Workflow

1. Start the app.
2. Resolve or configure the PostgreSQL connection string.
3. Apply migrations and seed the database if it is empty.
4. Sign in with the bootstrap account or locally saved credentials.
5. Use the main shell to work with Dashboard, Products, Orders, Reports, and Settings.

## Implemented Screens

- `LoginWindow`
- `DatabaseSetupWindow`
- `MainWindow`
- `DashboardPage`
- `ProductsPage`
- `ProductEditPage`
- `OrdersPage`
- `ReportsPage`
- `SettingsPage`

## Architectural Summary

- WinUI 3 desktop client
- MVVM view models per screen
- EF Core + PostgreSQL for persistence
- Repository layer for entity operations
- Service layer for startup, auth, settings, navigation, import, dashboard, and reporting
- Manual service composition in `Services/AppBootstrapper.cs`

## Data Summary

- Seeded categories: 5
- Seeded products: 50
- Seeded orders: 180
- Packaged product images: 150

## Important Notes

- Automatic database setup is built into startup.
- The app still uses legacy product schema fields as generic accessory spec slots.
- Orders keep stock synchronized.
- Saved login credentials are encrypted in local app settings.
