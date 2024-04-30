using Application.Interfaces;
using Application.ViewModels.DashboardViewModels;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace Application.Services
{
    public class RevenueService : IRevenueService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RevenueService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<RevenueViewModel> GetRevenueAsync()
        {
            var newOrder = await _unitOfWork.OrderRepository.GetAsync(isTakeAll: true, expression: x => x.OrderStatus >= OrderStatus.Paid && x.OrderStatus != OrderStatus.Failed);
            double totalOrderIncome = newOrder.Items.Sum(item => item.TotalPrice);
            var newServiceOrder = await _unitOfWork.ServiceOrderRepository.GetAsync(isTakeAll: true, expression: x => x.ServiceOrderStatus >= Domain.Enums.ServiceOrderStatus.Paid && x.ServiceOrderStatus != ServiceOrderStatus.Fail && x.ServiceOrderStatus != ServiceOrderStatus.Canceled);
            double totalServiceOrderIncome = newServiceOrder.Items.Sum(item => item.TotalPrice);
            return new RevenueViewModel()
            {
                TotalOrderIncome = totalOrderIncome,
                TotalServiceIncome = totalServiceOrderIncome
            };
        }
        public async Task<byte[]> GetExcel()
        {
            var newOrder = await _unitOfWork.OrderRepository
                .GetAllQueryable()
                .Where(x => x.OrderStatus >= OrderStatus.Paid && x.OrderStatus != OrderStatus.Failed)
                .Include(x => x.Customer)
                .ThenInclude(customer => customer.ApplicationUser)
                .ToListAsync();
            var newServiceOrder = await _unitOfWork.ServiceOrderRepository
                .GetAllQueryable()
                .Where(x => x.ServiceOrderStatus >= ServiceOrderStatus.Paid && x.ServiceOrderStatus != ServiceOrderStatus.Fail)
                .Include(x => x.CustomerGarden.Customer)
                .ThenInclude(customer => customer.ApplicationUser)
                .ToListAsync();
            using (var package = new ExcelPackage())
            {
                var orderWorksheet = package.Workbook.Worksheets.Add("Đơn hàng bonsai");
                PopulateOrderWorksheet(orderWorksheet, newOrder);
                var serviceOrderWorksheet = package.Workbook.Worksheets.Add("Đơn hàng dịch vụ");
                PopulateServiceOrderWorksheet(serviceOrderWorksheet, newServiceOrder);
                byte[] excelBytes = package.GetAsByteArray();
                return excelBytes;
            }
        }
        private void PopulateOrderWorksheet(ExcelWorksheet worksheet, IEnumerable<Order> orders)
        {
            worksheet.Cells[1, 1].Value = "Tên khách hàng";
            worksheet.Cells[1, 2].Value = "Địa chỉ";
            worksheet.Cells[1, 3].Value = "Ngày đặt hàng";
            worksheet.Cells[1, 4].Value = "Ngày giao dự kiến";
            worksheet.Cells[1, 5].Value = "Ngày giao";
            worksheet.Cells[1, 6].Value = "Tổng đơn hàng";
            worksheet.Cells[1, 7].Value = "Phí vận chuyển";
            worksheet.Cells[1, 8].Value = "Thành tiền";
            worksheet.Cells[1, 9].Value = "Trạng thái";
            int row = 2;
            double totalSum = 0;
            foreach (var order in orders)
            {
                worksheet.Cells[row, 1].Value = order.Customer.ApplicationUser.Fullname;
                worksheet.Cells[row, 2].Value = order.Address;
                worksheet.Cells[row, 3].Value = order.OrderDate.ToString("dd-MM-yyyy");
                worksheet.Cells[row, 4].Value = order.ExpectedDeliveryDate?.ToString("dd-MM-yyyy") ?? "";
                worksheet.Cells[row, 5].Value = order.DeliveryDate?.ToString("dd-MM-yyyy") ?? "";
                worksheet.Cells[row, 6].Value = order.Price;
                worksheet.Cells[row, 7].Value = order.DeliveryPrice;
                worksheet.Cells[row, 8].Value = order.TotalPrice;
                worksheet.Cells[row, 9].Value = order.OrderStatus.ToString();

                totalSum += order.TotalPrice;
                row++;
            }
            worksheet.Cells[row, 7].Value = "Tổng:";
            worksheet.Cells[row, 8].Value = totalSum;
        }
        private void PopulateServiceOrderWorksheet(ExcelWorksheet worksheet, IEnumerable<ServiceOrder> serviceOrders)
        {
            worksheet.Cells[1, 1].Value = "Tên khách hàng";
            worksheet.Cells[1, 2].Value = "Số điện thoại";
            worksheet.Cells[1, 3].Value = "Khoảng cách";
            worksheet.Cells[1, 4].Value = "Ngày bắt đầu";
            worksheet.Cells[1, 5].Value = "Ngày kết thức";
            worksheet.Cells[1, 6].Value = "Địa chỉ";
            worksheet.Cells[1, 7].Value = "Thành tiền";
            worksheet.Cells[1, 8].Value = "Trạng thái";

            double totalSum = 0;
            int row = 2;
            foreach (var serviceOrder in serviceOrders)
            {
                worksheet.Cells[row, 1].Value = serviceOrder.CustomerName;
                worksheet.Cells[row, 2].Value = serviceOrder.CustomerPhoneNumber;
                worksheet.Cells[row, 3].Value = serviceOrder.Distance;
                worksheet.Cells[row, 4].Value = serviceOrder.StartDate.ToString("dd-MM-yyyy");
                worksheet.Cells[row, 5].Value = serviceOrder.EndDate.ToString("dd-MM-yyyy");
                worksheet.Cells[row, 6].Value = serviceOrder.Address;
                worksheet.Cells[row, 7].Value = serviceOrder.TotalPrice;
                worksheet.Cells[row, 8].Value = serviceOrder.ServiceOrderStatus.ToString();

                totalSum += serviceOrder.TotalPrice;
                row++;
            }
            worksheet.Cells[row, 6].Value = "Tổng:";
            worksheet.Cells[row, 7].Value = totalSum;
        }
        public async Task<List<Order>> GetOrdersAsync()
        {
            var newOrder = await _unitOfWork.OrderRepository
                .GetAllQueryable()
                .Where(x => x.OrderStatus >= OrderStatus.Paid && x.OrderStatus != OrderStatus.Failed)
                .Include(x => x.Customer)
                .ThenInclude(customer => customer.ApplicationUser)
                .ToListAsync();
            return newOrder;
        }
        public async Task<List<ServiceOrder>> GetServiceOrdersAsync()
        {
            var newServiceOrder = await _unitOfWork.ServiceOrderRepository
               .GetAllQueryable()
               .Where(x => x.CreationDate >= DateTime.Now.AddDays(-30) && x.ServiceOrderStatus >= ServiceOrderStatus.Paid && x.ServiceOrderStatus != ServiceOrderStatus.Fail)
               .Include(x => x.CustomerGarden.Customer)
               .ThenInclude(customer => customer.ApplicationUser)
               .Include(x => x.Service)
               .ToListAsync();
            return newServiceOrder;
        }
    }
}
