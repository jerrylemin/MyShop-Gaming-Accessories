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
            entity.Property(x => x.FinalPrice).HasColumnName("final_price").HasColumnType("numeric(12,2)");
            entity.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(40);
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
    }
}
