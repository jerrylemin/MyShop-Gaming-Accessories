using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ProjectTest.DataAccess.Migrations;

[DbContext(typeof(MyShopDbContext))]
[Migration("20260309120000_InitialCreate")]
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "categories",
            columns: table => new
            {
                category_id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_categories", x => x.category_id);
            });

        migrationBuilder.CreateTable(
            name: "orders",
            columns: table => new
            {
                order_id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                created_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                final_price = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_orders", x => x.order_id);
            });

        migrationBuilder.CreateTable(
            name: "products",
            columns: table => new
            {
                product_id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                sku = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                manufacturer = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                cpu = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                ram = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                storage = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                gpu = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                screen = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                import_price = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                sale_price = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                count = table.Column<int>(type: "integer", nullable: false),
                category_id = table.Column<int>(type: "integer", nullable: false),
                description = table.Column<string>(type: "character varying(1200)", maxLength: 1200, nullable: false),
                image1 = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                image2 = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                image3 = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_products", x => x.product_id);
                table.ForeignKey(
                    name: "FK_products_categories_category_id",
                    column: x => x.category_id,
                    principalTable: "categories",
                    principalColumn: "category_id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "order_items",
            columns: table => new
            {
                order_item_id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                order_id = table.Column<int>(type: "integer", nullable: false),
                product_id = table.Column<int>(type: "integer", nullable: false),
                quantity = table.Column<int>(type: "integer", nullable: false),
                unit_sale_price = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                total_price = table.Column<decimal>(type: "numeric(12,2)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_order_items", x => x.order_item_id);
                table.ForeignKey(
                    name: "FK_order_items_orders_order_id",
                    column: x => x.order_id,
                    principalTable: "orders",
                    principalColumn: "order_id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_order_items_products_product_id",
                    column: x => x.product_id,
                    principalTable: "products",
                    principalColumn: "product_id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_order_items_order_id",
            table: "order_items",
            column: "order_id");

        migrationBuilder.CreateIndex(
            name: "IX_order_items_product_id",
            table: "order_items",
            column: "product_id");

        migrationBuilder.CreateIndex(
            name: "IX_products_category_id",
            table: "products",
            column: "category_id");

        migrationBuilder.CreateIndex(
            name: "IX_products_sku",
            table: "products",
            column: "sku",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "order_items");

        migrationBuilder.DropTable(
            name: "orders");

        migrationBuilder.DropTable(
            name: "products");

        migrationBuilder.DropTable(
            name: "categories");
    }
}
