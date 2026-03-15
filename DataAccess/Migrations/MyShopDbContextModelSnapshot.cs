using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using ProjectTest.DataAccess;

#nullable disable

namespace ProjectTest.DataAccess.Migrations;

[DbContext(typeof(MyShopDbContext))]
partial class MyShopDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasAnnotation("ProductVersion", "8.0.22")
            .HasAnnotation("Relational:MaxIdentifierLength", 63);

        modelBuilder.UseIdentityByDefaultColumns();

        modelBuilder.Entity("ProjectTest.Models.Category", b =>
        {
            b.Property<int>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("integer")
                .HasColumnName("category_id");

            NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

            b.Property<string>("Description")
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnType("character varying(500)")
                .HasColumnName("description");

            b.Property<string>("Name")
                .IsRequired()
                .HasMaxLength(120)
                .HasColumnType("character varying(120)")
                .HasColumnName("name");

            b.HasKey("Id");

            b.ToTable("categories", (string)null);
        });

        modelBuilder.Entity("ProjectTest.Models.Order", b =>
        {
            b.Property<int>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("integer")
                .HasColumnName("order_id");

            NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

            b.Property<DateTime>("CreatedTime")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_time");

            b.Property<decimal>("FinalPrice")
                .HasColumnType("numeric(12,2)")
                .HasColumnName("final_price");

            b.Property<string>("Status")
                .IsRequired()
                .HasMaxLength(40)
                .HasColumnType("character varying(40)")
                .HasColumnName("status");

            b.HasKey("Id");

            b.ToTable("orders", (string)null);
        });

        modelBuilder.Entity("ProjectTest.Models.Product", b =>
        {
            b.Property<int>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("integer")
                .HasColumnName("product_id");

            NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

            b.Property<int>("CategoryId")
                .HasColumnType("integer")
                .HasColumnName("category_id");

            b.Property<string>("CPU")
                .IsRequired()
                .HasMaxLength(120)
                .HasColumnType("character varying(120)")
                .HasColumnName("cpu");

            b.Property<string>("Description")
                .IsRequired()
                .HasMaxLength(1200)
                .HasColumnType("character varying(1200)")
                .HasColumnName("description");

            b.Property<string>("GPU")
                .IsRequired()
                .HasMaxLength(120)
                .HasColumnType("character varying(120)")
                .HasColumnName("gpu");

            b.Property<string>("Image1")
                .IsRequired()
                .HasMaxLength(260)
                .HasColumnType("character varying(260)")
                .HasColumnName("image1");

            b.Property<string>("Image2")
                .IsRequired()
                .HasMaxLength(260)
                .HasColumnType("character varying(260)")
                .HasColumnName("image2");

            b.Property<string>("Image3")
                .IsRequired()
                .HasMaxLength(260)
                .HasColumnType("character varying(260)")
                .HasColumnName("image3");

            b.Property<decimal>("ImportPrice")
                .HasColumnType("numeric(12,2)")
                .HasColumnName("import_price");

            b.Property<string>("Manufacturer")
                .IsRequired()
                .HasMaxLength(80)
                .HasColumnType("character varying(80)")
                .HasColumnName("manufacturer");

            b.Property<string>("Name")
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnType("character varying(200)")
                .HasColumnName("name");

            b.Property<string>("RAM")
                .IsRequired()
                .HasMaxLength(40)
                .HasColumnType("character varying(40)")
                .HasColumnName("ram");

            b.Property<decimal>("SalePrice")
                .HasColumnType("numeric(12,2)")
                .HasColumnName("sale_price");

            b.Property<string>("SKU")
                .IsRequired()
                .HasMaxLength(40)
                .HasColumnType("character varying(40)")
                .HasColumnName("sku");

            b.Property<string>("Screen")
                .IsRequired()
                .HasMaxLength(80)
                .HasColumnType("character varying(80)")
                .HasColumnName("screen");

            b.Property<int>("Stock")
                .HasColumnType("integer")
                .HasColumnName("count");

            b.Property<string>("Storage")
                .IsRequired()
                .HasMaxLength(80)
                .HasColumnType("character varying(80)")
                .HasColumnName("storage");

            b.HasKey("Id");

            b.HasIndex("CategoryId");

            b.HasIndex("SKU")
                .IsUnique();

            b.ToTable("products", (string)null);
        });

        modelBuilder.Entity("ProjectTest.Models.OrderItem", b =>
        {
            b.Property<int>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("integer")
                .HasColumnName("order_item_id");

            NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

            b.Property<int>("OrderId")
                .HasColumnType("integer")
                .HasColumnName("order_id");

            b.Property<int>("ProductId")
                .HasColumnType("integer")
                .HasColumnName("product_id");

            b.Property<int>("Quantity")
                .HasColumnType("integer")
                .HasColumnName("quantity");

            b.Property<decimal>("TotalPrice")
                .HasColumnType("numeric(12,2)")
                .HasColumnName("total_price");

            b.Property<decimal>("UnitSalePrice")
                .HasColumnType("numeric(12,2)")
                .HasColumnName("unit_sale_price");

            b.HasKey("Id");

            b.HasIndex("OrderId");

            b.HasIndex("ProductId");

            b.ToTable("order_items", (string)null);
        });

        modelBuilder.Entity("ProjectTest.Models.OrderItem", b =>
        {
            b.HasOne("ProjectTest.Models.Order", "Order")
                .WithMany("Items")
                .HasForeignKey("OrderId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            b.HasOne("ProjectTest.Models.Product", "Product")
                .WithMany("OrderItems")
                .HasForeignKey("ProductId")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            b.Navigation("Order");

            b.Navigation("Product");
        });

        modelBuilder.Entity("ProjectTest.Models.Product", b =>
        {
            b.HasOne("ProjectTest.Models.Category", "Category")
                .WithMany("Products")
                .HasForeignKey("CategoryId")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            b.Navigation("Category");
        });

        modelBuilder.Entity("ProjectTest.Models.Category", b =>
        {
            b.Navigation("Products");
        });

        modelBuilder.Entity("ProjectTest.Models.Order", b =>
        {
            b.Navigation("Items");
        });

        modelBuilder.Entity("ProjectTest.Models.Product", b =>
        {
            b.Navigation("OrderItems");
        });
#pragma warning restore 612, 618
    }
}
