using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructures.Migrations
{
    /// <inheritdoc />
    public partial class updateExpectedPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinHeight",
                table: "CategoryExpectedPrice");

            migrationBuilder.AddColumn<float>(
                name: "MaxHeight",
                table: "CategoryExpectedPrice",
                type: "real",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxHeight",
                table: "CategoryExpectedPrice");

            migrationBuilder.AddColumn<float>(
                name: "MinHeight",
                table: "CategoryExpectedPrice",
                type: "real",
                nullable: false,
                defaultValue: 0f);
        }
    }
}
