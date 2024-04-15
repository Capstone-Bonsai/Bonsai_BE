using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructures.Migrations
{
    /// <inheritdoc />
    public partial class UpdateServiceType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Image",
                table: "ServiceType",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "TypeEnum",
                table: "ServiceType",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.InsertData(
                table: "ServiceType",
                columns: new[] { "Id", "CreatedBy", "CreationDate", "DeleteBy", "DeletionDate", "Description", "Image", "IsDeleted", "ModificationBy", "ModificationDate", "TypeEnum", "TypeName" },
                values: new object[,]
                {
                    { new Guid("381e77b3-2cfa-4362-afae-fe588701616e"), null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "không có", "", false, null, null, 2, "Dịch vụ chăm sóc sân vườn" },
                    { new Guid("70f34b1c-1a2c-40ad-a9b6-ec374db61354"), null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "Không có", "", false, null, null, 1, "Dịch vụ chăm sóc bonsai" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ServiceType",
                keyColumn: "Id",
                keyValue: new Guid("381e77b3-2cfa-4362-afae-fe588701616e"));

            migrationBuilder.DeleteData(
                table: "ServiceType",
                keyColumn: "Id",
                keyValue: new Guid("70f34b1c-1a2c-40ad-a9b6-ec374db61354"));

            migrationBuilder.DropColumn(
                name: "Image",
                table: "ServiceType");

            migrationBuilder.DropColumn(
                name: "TypeEnum",
                table: "ServiceType");
        }
    }
}
