using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructures.Migrations
{
    /// <inheritdoc />
    public partial class NewMigrationService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StandardPrice",
                table: "Service");

            migrationBuilder.RenameColumn(
                name: "ContractStatus",
                table: "ServiceOrder",
                newName: "ServiceOrderStatus");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ServiceOrderStatus",
                table: "ServiceOrder",
                newName: "ContractStatus");

            migrationBuilder.AddColumn<double>(
                name: "StandardPrice",
                table: "Service",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
