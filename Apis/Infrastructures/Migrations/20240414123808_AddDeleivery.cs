using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructures.Migrations
{
    /// <inheritdoc />
    public partial class AddDeleivery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StaffId",
                table: "Order",
                newName: "GardenerId");

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpectedDeliveryDate",
                table: "Order",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "BaseTask",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Image",
                table: "BaseTask",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrderId",
                table: "BaseTask",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BaseTask_OrderId",
                table: "BaseTask",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_BaseTask_Order_OrderId",
                table: "BaseTask",
                column: "OrderId",
                principalTable: "Order",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BaseTask_Order_OrderId",
                table: "BaseTask");

            migrationBuilder.DropIndex(
                name: "IX_BaseTask_OrderId",
                table: "BaseTask");

            migrationBuilder.DropColumn(
                name: "ExpectedDeliveryDate",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "BaseTask");

            migrationBuilder.DropColumn(
                name: "Image",
                table: "BaseTask");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "BaseTask");

            migrationBuilder.RenameColumn(
                name: "GardenerId",
                table: "Order",
                newName: "StaffId");
        }
    }
}
