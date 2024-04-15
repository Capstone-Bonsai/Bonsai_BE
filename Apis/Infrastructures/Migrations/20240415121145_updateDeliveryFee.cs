using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructures.Migrations
{
    /// <inheritdoc />
    public partial class updateDeliveryFee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceOrder_ServiceType_ServiceTypeId",
                table: "ServiceOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceOrder_Service_ServiceId",
                table: "ServiceOrder");

            migrationBuilder.DropIndex(
                name: "IX_ServiceOrder_ServiceTypeId",
                table: "ServiceOrder");

            migrationBuilder.DropColumn(
                name: "NumberOfGardener",
                table: "ServiceOrder");

            migrationBuilder.DropColumn(
                name: "ServiceTypeId",
                table: "ServiceOrder");

            migrationBuilder.RenameColumn(
                name: "DeliveryType",
                table: "DeliveryFee",
                newName: "DeliverySize");

            migrationBuilder.AlterColumn<Guid>(
                name: "ServiceId",
                table: "ServiceOrder",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "MaxDistance",
                table: "DeliveryFee",
                type: "float",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceOrder_Service_ServiceId",
                table: "ServiceOrder",
                column: "ServiceId",
                principalTable: "Service",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceOrder_Service_ServiceId",
                table: "ServiceOrder");

            migrationBuilder.RenameColumn(
                name: "DeliverySize",
                table: "DeliveryFee",
                newName: "DeliveryType");

            migrationBuilder.AlterColumn<Guid>(
                name: "ServiceId",
                table: "ServiceOrder",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<int>(
                name: "NumberOfGardener",
                table: "ServiceOrder",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "ServiceTypeId",
                table: "ServiceOrder",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<int>(
                name: "MaxDistance",
                table: "DeliveryFee",
                type: "int",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceOrder_ServiceTypeId",
                table: "ServiceOrder",
                column: "ServiceTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceOrder_ServiceType_ServiceTypeId",
                table: "ServiceOrder",
                column: "ServiceTypeId",
                principalTable: "ServiceType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceOrder_Service_ServiceId",
                table: "ServiceOrder",
                column: "ServiceId",
                principalTable: "Service",
                principalColumn: "Id");
        }
    }
}
