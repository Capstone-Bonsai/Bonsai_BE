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
                name: "FK_Address_Customer_CustomerId",
                table: "Address");

            migrationBuilder.DropForeignKey(
                name: "FK_AnnualWorkingDay_Gardener_GardenerId",
                table: "AnnualWorkingDay");

            migrationBuilder.DropForeignKey(
                name: "FK_AnnualWorkingDay_OrderService_ServiceOrderId",
                table: "AnnualWorkingDay");

            migrationBuilder.DropForeignKey(
                name: "FK_AnnualWorkingDay_Shift_ShiftId",
                table: "AnnualWorkingDay");

            migrationBuilder.DropForeignKey(
                name: "FK_BaseTask_Service_ServiceId",
                table: "BaseTask");

            migrationBuilder.DropForeignKey(
                name: "FK_BaseTask_Task_TaskId",
                table: "BaseTask");

            migrationBuilder.DropForeignKey(
                name: "FK_Complain_OrderService_ServiceOrderId",
                table: "Complain");

            migrationBuilder.DropForeignKey(
                name: "FK_ContractImage_OrderService_ServiceOrderId",
                table: "ContractImage");

            migrationBuilder.DropForeignKey(
                name: "FK_Customer_AspNetUsers_UserId",
                table: "Customer");

            migrationBuilder.DropForeignKey(
                name: "FK_Gardener_AspNetUsers_UserId",
                table: "Gardener");

            migrationBuilder.DropForeignKey(
                name: "FK_Manager_AspNetUsers_UserId",
                table: "Manager");

            migrationBuilder.DropForeignKey(
                name: "FK_Order_Customer_CustomerId",
                table: "Order");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetail_Order_OrderId",
                table: "OrderDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetail_Product_ProductId",
                table: "OrderDetail");

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
                name: "FK_OrderServiceTask_Task_TaskId",
                table: "OrderServiceTask");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderTransaction_Order_OrderId",
                table: "OrderTransaction");

            migrationBuilder.DropForeignKey(
                name: "FK_Product_SubCategory_SubCategoryId",
                table: "Product");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductImage_Product_ProductId",
                table: "ProductImage");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductTag_Product_ProductId",
                table: "ProductTag");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductTag_Tag_TagId",
                table: "ProductTag");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceImage_OrderService_ServiceOrderId",
                table: "ServiceImage");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceTransaction_OrderService_ServiceOrderId",
                table: "ServiceTransaction");

            migrationBuilder.DropForeignKey(
                name: "FK_Staff_AspNetUsers_UserId",
                table: "Staff");

            migrationBuilder.DropForeignKey(
                name: "FK_SubCategory_Category_CategoryId",
                table: "SubCategory");

            migrationBuilder.AddForeignKey(
                name: "FK_Address_Customer_CustomerId",
                table: "Address",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AnnualWorkingDay_Gardener_GardenerId",
                table: "AnnualWorkingDay",
                column: "GardenerId",
                principalTable: "Gardener",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AnnualWorkingDay_OrderService_ServiceOrderId",
                table: "AnnualWorkingDay",
                column: "ServiceOrderId",
                principalTable: "OrderService",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AnnualWorkingDay_Shift_ShiftId",
                table: "AnnualWorkingDay",
                column: "ShiftId",
                principalTable: "Shift",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BaseTask_Service_ServiceId",
                table: "BaseTask",
                column: "ServiceId",
                principalTable: "Service",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BaseTask_Task_TaskId",
                table: "BaseTask",
                column: "TaskId",
                principalTable: "Task",
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
                name: "FK_Customer_AspNetUsers_UserId",
                table: "Customer",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Gardener_AspNetUsers_UserId",
                table: "Gardener",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Manager_AspNetUsers_UserId",
                table: "Manager",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Order_Customer_CustomerId",
                table: "Order",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetail_Order_OrderId",
                table: "OrderDetail",
                column: "OrderId",
                principalTable: "Order",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetail_Product_ProductId",
                table: "OrderDetail",
                column: "ProductId",
                principalTable: "Product",
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
                name: "FK_OrderServiceTask_Task_TaskId",
                table: "OrderServiceTask",
                column: "TaskId",
                principalTable: "Task",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderTransaction_Order_OrderId",
                table: "OrderTransaction",
                column: "OrderId",
                principalTable: "Order",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Product_SubCategory_SubCategoryId",
                table: "Product",
                column: "SubCategoryId",
                principalTable: "SubCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductImage_Product_ProductId",
                table: "ProductImage",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductTag_Product_ProductId",
                table: "ProductTag",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductTag_Tag_TagId",
                table: "ProductTag",
                column: "TagId",
                principalTable: "Tag",
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

            migrationBuilder.AddForeignKey(
                name: "FK_Staff_AspNetUsers_UserId",
                table: "Staff",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SubCategory_Category_CategoryId",
                table: "SubCategory",
                column: "CategoryId",
                principalTable: "Category",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Address_Customer_CustomerId",
                table: "Address");

            migrationBuilder.DropForeignKey(
                name: "FK_AnnualWorkingDay_Gardener_GardenerId",
                table: "AnnualWorkingDay");

            migrationBuilder.DropForeignKey(
                name: "FK_AnnualWorkingDay_OrderService_ServiceOrderId",
                table: "AnnualWorkingDay");

            migrationBuilder.DropForeignKey(
                name: "FK_AnnualWorkingDay_Shift_ShiftId",
                table: "AnnualWorkingDay");

            migrationBuilder.DropForeignKey(
                name: "FK_BaseTask_Service_ServiceId",
                table: "BaseTask");

            migrationBuilder.DropForeignKey(
                name: "FK_BaseTask_Task_TaskId",
                table: "BaseTask");

            migrationBuilder.DropForeignKey(
                name: "FK_Complain_OrderService_ServiceOrderId",
                table: "Complain");

            migrationBuilder.DropForeignKey(
                name: "FK_ContractImage_OrderService_ServiceOrderId",
                table: "ContractImage");

            migrationBuilder.DropForeignKey(
                name: "FK_Customer_AspNetUsers_UserId",
                table: "Customer");

            migrationBuilder.DropForeignKey(
                name: "FK_Gardener_AspNetUsers_UserId",
                table: "Gardener");

            migrationBuilder.DropForeignKey(
                name: "FK_Manager_AspNetUsers_UserId",
                table: "Manager");

            migrationBuilder.DropForeignKey(
                name: "FK_Order_Customer_CustomerId",
                table: "Order");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetail_Order_OrderId",
                table: "OrderDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetail_Product_ProductId",
                table: "OrderDetail");

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
                name: "FK_OrderServiceTask_Task_TaskId",
                table: "OrderServiceTask");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderTransaction_Order_OrderId",
                table: "OrderTransaction");

            migrationBuilder.DropForeignKey(
                name: "FK_Product_SubCategory_SubCategoryId",
                table: "Product");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductImage_Product_ProductId",
                table: "ProductImage");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductTag_Product_ProductId",
                table: "ProductTag");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductTag_Tag_TagId",
                table: "ProductTag");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceImage_OrderService_ServiceOrderId",
                table: "ServiceImage");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceTransaction_OrderService_ServiceOrderId",
                table: "ServiceTransaction");

            migrationBuilder.DropForeignKey(
                name: "FK_Staff_AspNetUsers_UserId",
                table: "Staff");

            migrationBuilder.DropForeignKey(
                name: "FK_SubCategory_Category_CategoryId",
                table: "SubCategory");

            migrationBuilder.AddForeignKey(
                name: "FK_Address_Customer_CustomerId",
                table: "Address",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AnnualWorkingDay_Gardener_GardenerId",
                table: "AnnualWorkingDay",
                column: "GardenerId",
                principalTable: "Gardener",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AnnualWorkingDay_OrderService_ServiceOrderId",
                table: "AnnualWorkingDay",
                column: "ServiceOrderId",
                principalTable: "OrderService",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AnnualWorkingDay_Shift_ShiftId",
                table: "AnnualWorkingDay",
                column: "ShiftId",
                principalTable: "Shift",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BaseTask_Service_ServiceId",
                table: "BaseTask",
                column: "ServiceId",
                principalTable: "Service",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BaseTask_Task_TaskId",
                table: "BaseTask",
                column: "TaskId",
                principalTable: "Task",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Complain_OrderService_ServiceOrderId",
                table: "Complain",
                column: "ServiceOrderId",
                principalTable: "OrderService",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ContractImage_OrderService_ServiceOrderId",
                table: "ContractImage",
                column: "ServiceOrderId",
                principalTable: "OrderService",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Customer_AspNetUsers_UserId",
                table: "Customer",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Gardener_AspNetUsers_UserId",
                table: "Gardener",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Manager_AspNetUsers_UserId",
                table: "Manager",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Order_Customer_CustomerId",
                table: "Order",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetail_Order_OrderId",
                table: "OrderDetail",
                column: "OrderId",
                principalTable: "Order",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetail_Product_ProductId",
                table: "OrderDetail",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderService_Customer_CustomerId",
                table: "OrderService",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderService_Service_ServiceId",
                table: "OrderService",
                column: "ServiceId",
                principalTable: "Service",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderServiceTask_OrderService_ServiceOrderId",
                table: "OrderServiceTask",
                column: "ServiceOrderId",
                principalTable: "OrderService",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderServiceTask_Task_TaskId",
                table: "OrderServiceTask",
                column: "TaskId",
                principalTable: "Task",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderTransaction_Order_OrderId",
                table: "OrderTransaction",
                column: "OrderId",
                principalTable: "Order",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Product_SubCategory_SubCategoryId",
                table: "Product",
                column: "SubCategoryId",
                principalTable: "SubCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductImage_Product_ProductId",
                table: "ProductImage",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductTag_Product_ProductId",
                table: "ProductTag",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductTag_Tag_TagId",
                table: "ProductTag",
                column: "TagId",
                principalTable: "Tag",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceImage_OrderService_ServiceOrderId",
                table: "ServiceImage",
                column: "ServiceOrderId",
                principalTable: "OrderService",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceTransaction_OrderService_ServiceOrderId",
                table: "ServiceTransaction",
                column: "ServiceOrderId",
                principalTable: "OrderService",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Staff_AspNetUsers_UserId",
                table: "Staff",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SubCategory_Category_CategoryId",
                table: "SubCategory",
                column: "CategoryId",
                principalTable: "Category",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
