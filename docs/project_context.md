# Project Context

## Current Domain
The application is now scoped to a Vietnamese gaming accessories store POS. The catalog includes keyboards, mice, headsets, mousepads, webcams, and microphones grouped into five seeded categories.

## Data Strategy
- Product names, brands, descriptions, and VND prices come from real Phong Vu product pages.
- The generated dataset is stored at `DataAccess/Seeding/gaming_accessories_seed_data.json`.
- Product images are built from public Unsplash downloads and copied into `Assets/GamingProducts`.

## Compatibility Constraints
- The WinUI 3, MVVM, repository, and EF Core architecture is unchanged.
- The existing `products` table schema is unchanged.
- Legacy domain-specific spec columns are reused as generic accessory spec slots to avoid a disruptive migration.

## Expected Runtime Outcome
- Fresh database seed contains 5 categories and 50 products.
- Products page shows image, product name, brand, price, and stock.
- Dashboard and reports continue to work from the same order tables and product relationships.
