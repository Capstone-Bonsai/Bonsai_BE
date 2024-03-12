using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructures.Migrations
{
    /// <inheritdoc />
    public partial class updateContract2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Distance",
                table: "Contract");

            migrationBuilder.RenameColumn(
                name: "MainBranch",
                table: "Bonsai",
                newName: "NumberOfTrunk");

            migrationBuilder.AddColumn<string>(
                name: "CustomerName",
                table: "Contract",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CustomerPhoneNumber",
                table: "Contract",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerName",
                table: "Contract");

            migrationBuilder.DropColumn(
                name: "CustomerPhoneNumber",
                table: "Contract");

            migrationBuilder.RenameColumn(
                name: "NumberOfTrunk",
                table: "Bonsai",
                newName: "MainBranch");

            migrationBuilder.AddColumn<int>(
                name: "Distance",
                table: "Contract",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
