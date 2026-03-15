# Database Schema

## Purpose
This document describes the EF Core model, PostgreSQL tables, relationships, and the gaming accessories seed behavior used by the WinUI 3 POS app.

## Provider And Entry Files
- Provider: PostgreSQL via Npgsql
- DbContext: `DataAccess/MyShopDbContext.cs`
- Runtime factory: `DataAccess/MyShopDbContextFactory.cs`
- Design-time factory: `DataAccess/DesignTimeMyShopDbContextFactory.cs`
- Runtime initializer: `Services/DatabaseInitializer.cs`
- Seed generator: `DataAccess/Seeding/GamingAccessorySeedGenerator.cs`
- Seed dataset resource: `DataAccess/Seeding/gaming_accessories_seed_data.json`

## Tables And Entities

### `categories`
- Model: `Models/Category.cs`
- Key: `category_id`
- Columns:
  - `name`
  - `description`
- Seed values:
  - `Gaming Keyboard`
  - `Gaming Mouse`
  - `Gaming Headset`
  - `Mousepad`
  - `Streaming Gear`

### `products`
- Model: `Models/Product.cs`
- Key: `product_id`
- Columns:
  - `sku`
  - `name`
  - `manufacturer`
  - `cpu`
  - `ram`
  - `storage`
  - `gpu`
  - `screen`
  - `import_price`
  - `sale_price`
  - `count`
  - `category_id`
  - `description`
  - `image1`
  - `image2`
  - `image3`
- Compatibility note:
  the legacy domain-specific spec columns are intentionally retained to avoid a schema migration. In the gaming accessories domain they now store five generic accessory spec lines pulled from retailer data.
- UI mapping:
  - `manufacturer` is shown as Brand
  - `cpu`, `ram`, `storage`, `gpu`, `screen` are surfaced as generic accessory spec entries

### `orders`
- Model: `Models/Order.cs`
- Key: `order_id`
- Columns:
  - `created_time`
  - `final_price`
  - `status`

### `order_items`
- Model: `Models/OrderItem.cs`
- Key: `order_item_id`
- Columns:
  - `order_id`
  - `product_id`
  - `quantity`
  - `unit_sale_price`
  - `total_price`

## Relationships
- `categories (1) -> products (many)`
- `products (1) -> order_items (many)`
- `orders (1) -> order_items (many)`

Configured in `DataAccess/MyShopDbContext.cs` with:
- `DeleteBehavior.Restrict` for category to product
- `DeleteBehavior.Restrict` for product to order item
- `DeleteBehavior.Cascade` for order to order item

## Enum Mapping
- Enum: `Models/OrderStatus.cs`
- Values:
  - `Created`
  - `Paid`
  - `Cancelled`
- Storage:
  string in `orders.status`

## EF Core Notes
- Table names and column names use snake_case.
- `products.sku` is unique.
- Price columns use `numeric(12,2)`.
- Existing PascalCase legacy schemas are normalized in `Services/DatabaseInitializer.cs` before migrations run.

## Seed Behavior
- Categories seeded: 5
- Products seeded: 50
- Orders seeded: 180
- Price source: real Vietnamese retail prices captured from Phong Vu into the embedded JSON dataset
- Stock quantity:
  randomized from 5 to 50 during seed generation
- Packaged images:
  each seeded product receives
  - `ms-appx:///Assets/GamingProducts/{productId}_1.jpg`
  - `ms-appx:///Assets/GamingProducts/{productId}_2.jpg`
  - `ms-appx:///Assets/GamingProducts/{productId}_3.jpg`

## Runtime Connection Defaults
- Environment variable override: `MYSHOP_CONNECTION_STRING`
- Saved local setting fallback: `DatabaseConnectionString`
- Built-in default database:
  `myshop_gaming_accessories`
