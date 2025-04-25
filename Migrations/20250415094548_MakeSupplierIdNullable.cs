using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SingularSystems_SelfKiosk_Software.Migrations
{
    /// <inheritdoc />
    public partial class MakeSupplierIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderItemId",
                table: "Products");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrderItemId",
                table: "Products",
                type: "int",
                nullable: true);
        }
    }
}
