using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectTest.DataAccess.Migrations;

public partial class AddOrderItemUnitCostPrice : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<decimal>(
            name: "unit_cost_price",
            table: "order_items",
            type: "numeric(12,2)",
            nullable: false,
            defaultValue: 0m);

        migrationBuilder.Sql("""
            UPDATE order_items oi
            SET unit_cost_price = COALESCE(p.import_price, 0)
            FROM products p
            WHERE p.product_id = oi.product_id
              AND (oi.unit_cost_price IS NULL OR oi.unit_cost_price = 0);
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "unit_cost_price",
            table: "order_items");
    }
}
