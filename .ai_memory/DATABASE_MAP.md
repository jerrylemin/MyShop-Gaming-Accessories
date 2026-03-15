# DATABASE_MAP

## Tables
- `categories`
  - key: `category_id`
  - columns: `name`, `description`
- `products`
  - key: `product_id`
  - columns: `sku`, `name`, `manufacturer`, `cpu`, `ram`, `storage`, `gpu`, `screen`, `import_price`, `sale_price`, `count`, `category_id`, `description`, `image1`, `image2`, `image3`
- `orders`
  - key: `order_id`
  - columns: `created_time`, `final_price`, `status`
- `order_items`
  - key: `order_item_id`
  - columns: `order_id`, `product_id`, `quantity`, `unit_sale_price`, `total_price`

## Relationships
- `categories.category_id -> products.category_id`
- `products.product_id -> order_items.product_id`
- `orders.order_id -> order_items.order_id`

## Domain Mapping
- `products.manufacturer` = Brand
- `products.cpu/ram/storage/gpu/screen` = generic accessory spec slots used by the current UI
- `products.sale_price` = customer-facing VND price
- `products.count` = stock quantity

## Seed Sources
- Seed generator: `DataAccess/Seeding/GamingAccessorySeedGenerator.cs`
- Embedded dataset: `DataAccess/Seeding/gaming_accessories_seed_data.json`
- Packaged images: `Assets/GamingProducts`
