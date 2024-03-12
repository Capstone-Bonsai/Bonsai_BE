using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructures.Migrations
{
    /// <inheritdoc />
    public partial class UpdateService3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contract_CustomerGarden_CustomerGardenId",
                table: "Contract");

            migrationBuilder.DropForeignKey(
                name: "FK_Contract_Service_ServiceId",
                table: "Contract");

            migrationBuilder.DropIndex(
                name: "IX_Contract_CustomerGardenId",
                table: "Contract");

            migrationBuilder.DropColumn(
                name: "CustomerGardenStatus",
                table: "CustomerGarden");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "CustomerGarden");

            migrationBuilder.DropColumn(
                name: "TemporaryPrice",
                table: "CustomerGarden");

            migrationBuilder.DropColumn(
                name: "TemporarySurchargePrice",
                table: "CustomerGarden");

            migrationBuilder.DropColumn(
                name: "TemporaryTotalPrice",
                table: "CustomerGarden");

            migrationBuilder.DropColumn(
                name: "CustomerBonsaiStatus",
                table: "CustomerBonsai");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "CustomerBonsai");

            migrationBuilder.DropColumn(
                name: "TemporaryPrice",
                table: "CustomerBonsai");

            migrationBuilder.DropColumn(
                name: "TemporarySurchargePrice",
                table: "CustomerBonsai");

            migrationBuilder.DropColumn(
                name: "TemporaryTotalPrice",
                table: "CustomerBonsai");

            migrationBuilder.DropColumn(
                name: "CustomerGardenId",
                table: "Contract");

            migrationBuilder.RenameColumn(
                name: "ServiceId",
                table: "Contract",
                newName: "ServiceGardenId");

            migrationBuilder.RenameIndex(
                name: "IX_Contract_ServiceId",
                table: "Contract",
                newName: "IX_Contract_ServiceGardenId");

            migrationBuilder.CreateTable(
                name: "ServiceGarden",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerGardenId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TemporaryPrice = table.Column<double>(type: "float", nullable: true),
                    TemporarySurchargePrice = table.Column<double>(type: "float", nullable: true),
                    TemporaryTotalPrice = table.Column<double>(type: "float", nullable: true),
                    CustomerGardenStatus = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModificationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificationBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceGarden", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceGarden_CustomerGarden_CustomerGardenId",
                        column: x => x.CustomerGardenId,
                        principalTable: "CustomerGarden",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceGarden_Service_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Service",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceGarden_CustomerGardenId",
                table: "ServiceGarden",
                column: "CustomerGardenId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceGarden_ServiceId",
                table: "ServiceGarden",
                column: "ServiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contract_ServiceGarden_ServiceGardenId",
                table: "Contract",
                column: "ServiceGardenId",
                principalTable: "ServiceGarden",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contract_ServiceGarden_ServiceGardenId",
                table: "Contract");

            migrationBuilder.DropTable(
                name: "ServiceGarden");

            migrationBuilder.RenameColumn(
                name: "ServiceGardenId",
                table: "Contract",
                newName: "ServiceId");

            migrationBuilder.RenameIndex(
                name: "IX_Contract_ServiceGardenId",
                table: "Contract",
                newName: "IX_Contract_ServiceId");

            migrationBuilder.AddColumn<int>(
                name: "CustomerGardenStatus",
                table: "CustomerGarden",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "CustomerGarden",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "TemporaryPrice",
                table: "CustomerGarden",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "TemporarySurchargePrice",
                table: "CustomerGarden",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "TemporaryTotalPrice",
                table: "CustomerGarden",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CustomerBonsaiStatus",
                table: "CustomerBonsai",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "CustomerBonsai",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "TemporaryPrice",
                table: "CustomerBonsai",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "TemporarySurchargePrice",
                table: "CustomerBonsai",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "TemporaryTotalPrice",
                table: "CustomerBonsai",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerGardenId",
                table: "Contract",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Contract_CustomerGardenId",
                table: "Contract",
                column: "CustomerGardenId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contract_CustomerGarden_CustomerGardenId",
                table: "Contract",
                column: "CustomerGardenId",
                principalTable: "CustomerGarden",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Contract_Service_ServiceId",
                table: "Contract",
                column: "ServiceId",
                principalTable: "Service",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
