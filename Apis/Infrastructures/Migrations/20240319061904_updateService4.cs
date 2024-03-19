using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructures.Migrations
{
    /// <inheritdoc />
    public partial class updateService4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CategoryExpectedPrice_Category_CategoryId",
                table: "CategoryExpectedPrice");

            migrationBuilder.DropIndex(
                name: "IX_CategoryExpectedPrice_CategoryId",
                table: "CategoryExpectedPrice");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "CategoryExpectedPrice");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                table: "CategoryExpectedPrice",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_CategoryExpectedPrice_CategoryId",
                table: "CategoryExpectedPrice",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_CategoryExpectedPrice_Category_CategoryId",
                table: "CategoryExpectedPrice",
                column: "CategoryId",
                principalTable: "Category",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
