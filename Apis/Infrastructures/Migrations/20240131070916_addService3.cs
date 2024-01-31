using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructures.Migrations
{
    /// <inheritdoc />
    public partial class addService3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AnnualWorkingDay_OrderService_ServiceOrderId",
                table: "AnnualWorkingDay");

            migrationBuilder.DropForeignKey(
                name: "FK_Complain_OrderService_ServiceOrderId",
                table: "Complain");

            migrationBuilder.DropForeignKey(
                name: "FK_ContractImage_OrderService_ServiceOrderId",
                table: "ContractImage");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderService_Customer_CustomerId",
                table: "OrderService");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderService_Service_ServiceId",
                table: "OrderService");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderServiceTask_OrderService_ServiceOrderId",
                table: "OrderServiceTask");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceImage_OrderService_ServiceOrderId",
                table: "ServiceImage");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceTransaction_OrderService_ServiceOrderId",
                table: "ServiceTransaction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderService",
                table: "OrderService");

            migrationBuilder.RenameTable(
                name: "OrderService",
                newName: "ServiceOrder");

            migrationBuilder.RenameIndex(
                name: "IX_OrderService_ServiceId",
                table: "ServiceOrder",
                newName: "IX_ServiceOrder_ServiceId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderService_CustomerId",
                table: "ServiceOrder",
                newName: "IX_ServiceOrder_CustomerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ServiceOrder",
                table: "ServiceOrder",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AnnualWorkingDay_ServiceOrder_ServiceOrderId",
                table: "AnnualWorkingDay",
                column: "ServiceOrderId",
                principalTable: "ServiceOrder",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Complain_ServiceOrder_ServiceOrderId",
                table: "Complain",
                column: "ServiceOrderId",
                principalTable: "ServiceOrder",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ContractImage_ServiceOrder_ServiceOrderId",
                table: "ContractImage",
                column: "ServiceOrderId",
                principalTable: "ServiceOrder",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderServiceTask_ServiceOrder_ServiceOrderId",
                table: "OrderServiceTask",
                column: "ServiceOrderId",
                principalTable: "ServiceOrder",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceImage_ServiceOrder_ServiceOrderId",
                table: "ServiceImage",
                column: "ServiceOrderId",
                principalTable: "ServiceOrder",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceOrder_Customer_CustomerId",
                table: "ServiceOrder",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceOrder_Service_ServiceId",
                table: "ServiceOrder",
                column: "ServiceId",
                principalTable: "Service",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceTransaction_ServiceOrder_ServiceOrderId",
                table: "ServiceTransaction",
                column: "ServiceOrderId",
                principalTable: "ServiceOrder",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AnnualWorkingDay_ServiceOrder_ServiceOrderId",
                table: "AnnualWorkingDay");

            migrationBuilder.DropForeignKey(
                name: "FK_Complain_ServiceOrder_ServiceOrderId",
                table: "Complain");

            migrationBuilder.DropForeignKey(
                name: "FK_ContractImage_ServiceOrder_ServiceOrderId",
                table: "ContractImage");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderServiceTask_ServiceOrder_ServiceOrderId",
                table: "OrderServiceTask");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceImage_ServiceOrder_ServiceOrderId",
                table: "ServiceImage");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceOrder_Customer_CustomerId",
                table: "ServiceOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceOrder_Service_ServiceId",
                table: "ServiceOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceTransaction_ServiceOrder_ServiceOrderId",
                table: "ServiceTransaction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ServiceOrder",
                table: "ServiceOrder");

            migrationBuilder.RenameTable(
                name: "ServiceOrder",
                newName: "OrderService");

            migrationBuilder.RenameIndex(
                name: "IX_ServiceOrder_ServiceId",
                table: "OrderService",
                newName: "IX_OrderService_ServiceId");

            migrationBuilder.RenameIndex(
                name: "IX_ServiceOrder_CustomerId",
                table: "OrderService",
                newName: "IX_OrderService_CustomerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderService",
                table: "OrderService",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AnnualWorkingDay_OrderService_ServiceOrderId",
                table: "AnnualWorkingDay",
                column: "ServiceOrderId",
                principalTable: "OrderService",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Complain_OrderService_ServiceOrderId",
                table: "Complain",
                column: "ServiceOrderId",
                principalTable: "OrderService",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ContractImage_OrderService_ServiceOrderId",
                table: "ContractImage",
                column: "ServiceOrderId",
                principalTable: "OrderService",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderService_Customer_CustomerId",
                table: "OrderService",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderService_Service_ServiceId",
                table: "OrderService",
                column: "ServiceId",
                principalTable: "Service",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderServiceTask_OrderService_ServiceOrderId",
                table: "OrderServiceTask",
                column: "ServiceOrderId",
                principalTable: "OrderService",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceImage_OrderService_ServiceOrderId",
                table: "ServiceImage",
                column: "ServiceOrderId",
                principalTable: "OrderService",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceTransaction_OrderService_ServiceOrderId",
                table: "ServiceTransaction",
                column: "ServiceOrderId",
                principalTable: "OrderService",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
