using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ProjectTest.DataAccess.Migrations;

[DbContext(typeof(MyShopDbContext))]
[Migration("20260512120000_AdvancedPosFeatures")]
public partial class AdvancedPosFeatures : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(name: "created_by_user_id", table: "orders", type: "integer", nullable: true);
        migrationBuilder.AddColumn<int>(name: "customer_id", table: "orders", type: "integer", nullable: true);
        migrationBuilder.AddColumn<decimal>(name: "discount_amount", table: "orders", type: "numeric(12,2)", nullable: false, defaultValue: 0m);
        migrationBuilder.AddColumn<int>(name: "promotion_id", table: "orders", type: "integer", nullable: true);
        migrationBuilder.AddColumn<decimal>(name: "subtotal", table: "orders", type: "numeric(12,2)", nullable: false, defaultValue: 0m);

        migrationBuilder.CreateTable(
            name: "customers",
            columns: table => new
            {
                customer_id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                phone = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                email = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                loyalty_points = table.Column<int>(type: "integer", nullable: false),
                lifetime_spend = table.Column<decimal>(type: "numeric(12,2)", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_customers", x => x.customer_id));

        migrationBuilder.CreateTable(
            name: "promotions",
            columns: table => new
            {
                promotion_id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                code = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                discount_type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                discount_value = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                start_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                end_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                is_active = table.Column<bool>(type: "boolean", nullable: false),
                minimum_order_total = table.Column<decimal>(type: "numeric(12,2)", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_promotions", x => x.promotion_id));

        migrationBuilder.CreateTable(
            name: "users",
            columns: table => new
            {
                user_id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                username = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                display_name = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                role = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                is_active = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_users", x => x.user_id));

        migrationBuilder.CreateTable(
            name: "customer_loyalty_transactions",
            columns: table => new
            {
                loyalty_transaction_id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                customer_id = table.Column<int>(type: "integer", nullable: false),
                order_id = table.Column<int>(type: "integer", nullable: true),
                points = table.Column<int>(type: "integer", nullable: false),
                reason = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                created_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_customer_loyalty_transactions", x => x.loyalty_transaction_id);
                table.ForeignKey("FK_customer_loyalty_transactions_customers_customer_id", x => x.customer_id, "customers", "customer_id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_customer_loyalty_transactions_orders_order_id", x => x.order_id, "orders", "order_id", onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateIndex(name: "IX_orders_created_by_user_id", table: "orders", column: "created_by_user_id");
        migrationBuilder.CreateIndex(name: "IX_orders_customer_id", table: "orders", column: "customer_id");
        migrationBuilder.CreateIndex(name: "IX_orders_promotion_id", table: "orders", column: "promotion_id");
        migrationBuilder.CreateIndex(name: "IX_customer_loyalty_transactions_customer_id", table: "customer_loyalty_transactions", column: "customer_id");
        migrationBuilder.CreateIndex(name: "IX_customer_loyalty_transactions_order_id", table: "customer_loyalty_transactions", column: "order_id");
        migrationBuilder.CreateIndex(name: "IX_customers_phone", table: "customers", column: "phone");
        migrationBuilder.CreateIndex(name: "IX_promotions_code", table: "promotions", column: "code", unique: true);
        migrationBuilder.CreateIndex(name: "IX_users_username", table: "users", column: "username", unique: true);

        migrationBuilder.AddForeignKey("FK_orders_customers_customer_id", "orders", "customer_id", "customers", principalColumn: "customer_id", onDelete: ReferentialAction.SetNull);
        migrationBuilder.AddForeignKey("FK_orders_promotions_promotion_id", "orders", "promotion_id", "promotions", principalColumn: "promotion_id", onDelete: ReferentialAction.SetNull);
        migrationBuilder.AddForeignKey("FK_orders_users_created_by_user_id", "orders", "created_by_user_id", "users", principalColumn: "user_id", onDelete: ReferentialAction.SetNull);
        migrationBuilder.Sql("UPDATE orders SET subtotal = final_price WHERE subtotal = 0;");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "customer_loyalty_transactions");
        migrationBuilder.DropForeignKey("FK_orders_customers_customer_id", "orders");
        migrationBuilder.DropForeignKey("FK_orders_promotions_promotion_id", "orders");
        migrationBuilder.DropForeignKey("FK_orders_users_created_by_user_id", "orders");
        migrationBuilder.DropTable(name: "customers");
        migrationBuilder.DropTable(name: "promotions");
        migrationBuilder.DropTable(name: "users");
        migrationBuilder.DropColumn(name: "created_by_user_id", table: "orders");
        migrationBuilder.DropColumn(name: "customer_id", table: "orders");
        migrationBuilder.DropColumn(name: "discount_amount", table: "orders");
        migrationBuilder.DropColumn(name: "promotion_id", table: "orders");
        migrationBuilder.DropColumn(name: "subtotal", table: "orders");
    }
}
