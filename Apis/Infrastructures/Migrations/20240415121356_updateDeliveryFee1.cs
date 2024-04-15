using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructures.Migrations
{
    /// <inheritdoc />
    public partial class updateDeliveryFee1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DeliveryType",
                table: "Order",
                newName: "DeliverySize");

            migrationBuilder.RenameColumn(
                name: "DeliveryType",
                table: "Bonsai",
                newName: "DeliverySize");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DeliverySize",
                table: "Order",
                newName: "DeliveryType");

            migrationBuilder.RenameColumn(
                name: "DeliverySize",
                table: "Bonsai",
                newName: "DeliveryType");
        }
    }
}
