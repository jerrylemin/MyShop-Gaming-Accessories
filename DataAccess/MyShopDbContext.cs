using Microsoft.EntityFrameworkCore;
using ProjectTest.Models;

namespace ProjectTest.DataAccess;

public class MyShopDbContext : DbContext
{
    public MyShopDbContext(DbContextOptions<MyShopDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Product> Products => Set<Product>();

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    public DbSet<AppUser> Users => Set<AppUser>();

    public DbSet<Promotion> Promotions => Set<Promotion>();

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<CustomerLoyaltyTransaction> CustomerLoyaltyTransactions => Set<CustomerLoyaltyTransaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("categories");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("category_id");
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(120).IsRequired();
            entity.Property(x => x.Description).HasColumnName("description").HasMaxLength(500);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("products");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.SKU).IsUnique();
            entity.Property(x => x.Id).HasColumnName("product_id");
            entity.Property(x => x.SKU).HasColumnName("sku").HasMaxLength(40).IsRequired();
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            entity.Property(x => x.Manufacturer).HasColumnName("manufacturer").HasMaxLength(80).IsRequired();
            entity.Property(x => x.CPU).HasColumnName("cpu").HasMaxLength(120).IsRequired();
            entity.Property(x => x.RAM).HasColumnName("ram").HasMaxLength(40).IsRequired();
            entity.Property(x => x.Storage).HasColumnName("storage").HasMaxLength(80).IsRequired();
            entity.Property(x => x.GPU).HasColumnName("gpu").HasMaxLength(120).IsRequired();
            entity.Property(x => x.Screen).HasColumnName("screen").HasMaxLength(80).IsRequired();
            entity.Property(x => x.ImportPrice).HasColumnName("import_price").HasColumnType("numeric(12,2)");
            entity.Property(x => x.SalePrice).HasColumnName("sale_price").HasColumnType("numeric(12,2)");
            entity.Property(x => x.Stock).HasColumnName("count");
            entity.Property(x => x.CategoryId).HasColumnName("category_id");
            entity.Property(x => x.Description).HasColumnName("description").HasMaxLength(1200);
            entity.Property(x => x.Image1).HasColumnName("image1").HasMaxLength(260);
            entity.Property(x => x.Image2).HasColumnName("image2").HasMaxLength(260);
            entity.Property(x => x.Image3).HasColumnName("image3").HasMaxLength(260);
            entity.HasOne(x => x.Category)
                .WithMany(x => x.Products)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("orders");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("order_id");
            entity.Property(x => x.CreatedTime).HasColumnName("created_time").HasColumnType("timestamp without time zone");
            entity.Property(x => x.Subtotal).HasColumnName("subtotal").HasColumnType("numeric(12,2)");
            entity.Property(x => x.DiscountAmount).HasColumnName("discount_amount").HasColumnType("numeric(12,2)");
            entity.Property(x => x.FinalPrice).HasColumnName("final_price").HasColumnType("numeric(12,2)");
            entity.Property(x => x.PromotionId).HasColumnName("promotion_id");
            entity.Property(x => x.CustomerId).HasColumnName("customer_id");
            entity.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id");
            entity.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(40);
            entity.HasOne(x => x.Promotion)
                .WithMany()
                .HasForeignKey(x => x.PromotionId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.Customer)
                .WithMany(x => x.Orders)
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.CreatedByUser)
                .WithMany(x => x.Orders)
                .HasForeignKey(x => x.CreatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("order_items");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("order_item_id");
            entity.Property(x => x.OrderId).HasColumnName("order_id");
            entity.Property(x => x.ProductId).HasColumnName("product_id");
            entity.Property(x => x.Quantity).HasColumnName("quantity");
            entity.Property(x => x.UnitSalePrice).HasColumnName("unit_sale_price").HasColumnType("numeric(12,2)");
            entity.Property(x => x.UnitCostPrice).HasColumnName("unit_cost_price").HasColumnType("numeric(12,2)");
            entity.Property(x => x.TotalPrice).HasColumnName("total_price").HasColumnType("numeric(12,2)");
            entity.HasOne(x => x.Order)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Product)
                .WithMany(x => x.OrderItems)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Username).IsUnique();
            entity.Property(x => x.Id).HasColumnName("user_id");
            entity.Property(x => x.Username).HasColumnName("username").HasMaxLength(80).IsRequired();
            entity.Property(x => x.DisplayName).HasColumnName("display_name").HasMaxLength(160).IsRequired();
            entity.Property(x => x.Role).HasColumnName("role").HasConversion<string>().HasMaxLength(40);
            entity.Property(x => x.IsActive).HasColumnName("is_active");
        });

        modelBuilder.Entity<Promotion>(entity =>
        {
            entity.ToTable("promotions");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Code).IsUnique();
            entity.Property(x => x.Id).HasColumnName("promotion_id");
            entity.Property(x => x.Code).HasColumnName("code").HasMaxLength(40).IsRequired();
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(160).IsRequired();
            entity.Property(x => x.DiscountType).HasColumnName("discount_type").HasConversion<string>().HasMaxLength(40);
            entity.Property(x => x.DiscountValue).HasColumnName("discount_value").HasColumnType("numeric(12,2)");
            entity.Property(x => x.StartDate).HasColumnName("start_date").HasColumnType("timestamp without time zone");
            entity.Property(x => x.EndDate).HasColumnName("end_date").HasColumnType("timestamp without time zone");
            entity.Property(x => x.IsActive).HasColumnName("is_active");
            entity.Property(x => x.MinimumOrderTotal).HasColumnName("minimum_order_total").HasColumnType("numeric(12,2)");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("customers");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Phone);
            entity.Property(x => x.Id).HasColumnName("customer_id");
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(160).IsRequired();
            entity.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(40);
            entity.Property(x => x.Email).HasColumnName("email").HasMaxLength(160);
            entity.Property(x => x.LoyaltyPoints).HasColumnName("loyalty_points");
            entity.Property(x => x.LifetimeSpend).HasColumnName("lifetime_spend").HasColumnType("numeric(12,2)");
        });

        modelBuilder.Entity<CustomerLoyaltyTransaction>(entity =>
        {
            entity.ToTable("customer_loyalty_transactions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("loyalty_transaction_id");
            entity.Property(x => x.CustomerId).HasColumnName("customer_id");
            entity.Property(x => x.OrderId).HasColumnName("order_id");
            entity.Property(x => x.Points).HasColumnName("points");
            entity.Property(x => x.Reason).HasColumnName("reason").HasMaxLength(240).IsRequired();
            entity.Property(x => x.CreatedTime).HasColumnName("created_time").HasColumnType("timestamp without time zone");
            entity.HasOne(x => x.Customer)
                .WithMany()
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Order)
                .WithMany()
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
