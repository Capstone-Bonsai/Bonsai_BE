using Application.Interfaces;
using Application.Utils;
using Application.ViewModels.DashboardViewModels;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class DashBoardService : IDashBoardService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DashBoardService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<DashboardViewModel> GetDashboardAsync()
        {

            var newUser = await _unitOfWork.CustomerRepository.GetAsync(isTakeAll: true, expression: x => x.CreationDate >= DateTime.Now.AddDays(-30));
            var newOrder = await _unitOfWork.OrderRepository.GetAsync(isTakeAll: true, expression: x => x.CreationDate >= DateTime.Now.AddDays(-30) && x.OrderStatus >= Domain.Enums.OrderStatus.Paid && x.OrderStatus != Domain.Enums.OrderStatus.Failed);
            double totalOrderIncome = newOrder.Items.Sum(item => item.Price);
            var newServiceOrder = await _unitOfWork.ServiceOrderRepository.GetAsync(isTakeAll: true, expression: x => x.CreationDate >= DateTime.Now.AddDays(-30) && x.ServiceOrderStatus >= Domain.Enums.ServiceOrderStatus.Paid && x.ServiceOrderStatus != Domain.Enums.ServiceOrderStatus.Fail);
            double totalServiceOrderIncome = newServiceOrder.Items.Sum(item => item.TotalPrice);
            var currentServiceOrder = await _unitOfWork.ServiceOrderRepository.GetAsync(isTakeAll: true, expression: x => x.CreationDate >= DateTime.Now.AddDays(-30) && x.ServiceOrderStatus >= Domain.Enums.ServiceOrderStatus.Paid && x.ServiceOrderStatus != Domain.Enums.ServiceOrderStatus.Completed && x.ServiceOrderStatus != Domain.Enums.ServiceOrderStatus.Fail);
            DashboardViewModel dashboardViewModel = new DashboardViewModel()
            {
                NewUser = newUser.Items.Count,
                NewOrder = newOrder.Items.Count,
                TotalOrderIncome = totalOrderIncome,
                TotalServiceIncome = totalServiceOrderIncome,
                CurrentServiceOngoing = currentServiceOrder.Items.Count,
                OrderCircleGraphs = await GetOrderCircleGraph(totalOrderIncome),
                ServiceOrderCircleGraphs = await GetServiceOrderCircleGraph(totalServiceOrderIncome)
            };
            return dashboardViewModel;
        }
        private async Task<List<OrderCircleGraph>> GetOrderCircleGraph(double totalOrderIncome)
        {
            if (totalOrderIncome == 0) return new List<OrderCircleGraph>();
            List<OrderCircleGraph> orderCircleGraphs = new List<OrderCircleGraph>();
            var categories = await _unitOfWork.CategoryRepository.GetAsync(isTakeAll: true, expression: x => !x.IsDeleted);
            double previousPercentSum = 0;
            double currentPercent = 0;

            foreach (Category category in categories.Items)
            {
                var orderDetail = await _unitOfWork.OrderDetailRepository.GetAllQueryable()
                    .AsNoTracking()
                    .Where(x => x.Bonsai.CategoryId == category.Id && x.Order.OrderStatus >= Domain.Enums.OrderStatus.Paid && x.Order.OrderStatus != Domain.Enums.OrderStatus.Failed && x.CreationDate >= DateTime.Now.AddDays(-30))
                    .ToListAsync();
                if (category != categories.Items.Last())
                {
                    double totalCategoryOrderDetailPrice = orderDetail.Sum(x => x.Price);
                    double percent = (totalCategoryOrderDetailPrice / totalOrderIncome) * 100;
                    currentPercent = Math.Round(percent, 2);
                    previousPercentSum += currentPercent;
                    orderCircleGraphs.Add(new OrderCircleGraph()
                    {
                        CategoryName = category.Name,
                        Percent = currentPercent
                    });
                }
            }
            orderCircleGraphs.Add(new OrderCircleGraph()
            {
                CategoryName = categories.Items.Last().Name,
                Percent = Math.Round(100 - previousPercentSum, 2)
            });
            return orderCircleGraphs;
        }
        private async Task<List<ServiceOrderCircleGraph>> GetServiceOrderCircleGraph(double totalServiceOrderIncome)
        {
            if (totalServiceOrderIncome == 0) return new List<ServiceOrderCircleGraph>();
            List<ServiceOrderCircleGraph> serviceOrderCircleGraphs = new List<ServiceOrderCircleGraph>();
            var services = await _unitOfWork.ServiceRepository.GetAsync(isTakeAll: true, expression: x => !x.IsDeleted);
            double previousPercentSum = 0;
            double currentPercent = 0;

            foreach (Service service in services.Items)
            {
                var serviceOrder = await _unitOfWork.ServiceOrderRepository.GetAllQueryable()
                    .AsNoTracking()
                    .Where(x => x.Service.Id == service.Id && x.ServiceOrderStatus >= Domain.Enums.ServiceOrderStatus.Paid && x.CreationDate >= DateTime.Now.AddDays(-30) && x.ServiceOrderStatus != Domain.Enums.ServiceOrderStatus.Fail)
                    .ToListAsync();
                if (service != services.Items.Last())
                {
                    double totalServiceOrderPrice = serviceOrder.Sum(x => x.TotalPrice);
                    double percent = (totalServiceOrderPrice / totalServiceOrderIncome) * 100;
                    currentPercent = Math.Round(percent, 2);
                    previousPercentSum += currentPercent;
                    serviceOrderCircleGraphs.Add(new ServiceOrderCircleGraph()
                    {
                        ServiceName = service.Name,
                        Percent = currentPercent
                    });
                }
            }
            previousPercentSum = Math.Round(previousPercentSum, 2);
            serviceOrderCircleGraphs.Add(new ServiceOrderCircleGraph()
            {
                ServiceName = services.Items.Last().Name,
                Percent = Math.Round(100 - previousPercentSum, 2)
            });
            return serviceOrderCircleGraphs;
        }
        public async Task<List<RevenueLineGraph>> GetRevenueLineGraph()
        {
            List<RevenueLineGraph> revenueLineGraphs = new List<RevenueLineGraph>();

            var orders = await _unitOfWork.OrderRepository.GetAsync(
                isTakeAll: true,
                expression: x => x.CreationDate >= DateTime.Now.Date.AddMonths(-11) && x.CreationDate < DateTime.Now.Date &&
                                 x.OrderStatus >= Domain.Enums.OrderStatus.Paid
            );

            var serviceOrders = await _unitOfWork.ServiceOrderRepository.GetAsync(
                isTakeAll: true,
                expression: x => x.CreationDate >= DateTime.Now.Date.AddMonths(-11) && x.CreationDate < DateTime.Now.Date &&
                                 x.ServiceOrderStatus >= Domain.Enums.ServiceOrderStatus.Paid
            );
            var groupedOrders = orders.Items.GroupBy(o => new DateTime(o.CreationDate.Year, o.CreationDate.Month, 1));
            var groupedServiceOrders = serviceOrders.Items.GroupBy(so => new DateTime(so.CreationDate.Year, so.CreationDate.Month, 1));
            foreach (var month in Enumerable.Range(0, 12).Select(offset => DateTime.Now.Date.AddMonths(-11).AddMonths(offset)))
            {
                var monthStart = new DateTime(month.Year, month.Month, 1);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                double totalOrderIncome = groupedOrders.FirstOrDefault(g => g.Key >= monthStart && g.Key <= monthEnd)?.Sum(o => o.Price) ?? 0;
                double totalServiceOrderIncome = groupedServiceOrders.FirstOrDefault(g => g.Key >= monthStart && g.Key <= monthEnd)?.Sum(so => so.TotalPrice) ?? 0;

                revenueLineGraphs.Add(new RevenueLineGraph()
                {
                    time = month,
                    OrderTotal = totalOrderIncome,
                    ServiceOrderTotal = totalServiceOrderIncome
                });
            }
            return revenueLineGraphs;
        }
        public async Task<byte[]> GetExcel()
        {
            var newOrder = await _unitOfWork.OrderRepository
                .GetAllQueryable()
                .Where(x => x.CreationDate >= DateTime.Now.AddDays(-30) && x.OrderStatus >= OrderStatus.Paid && x.OrderStatus != OrderStatus.Failed)
                .Include(x => x.Customer)
                .ThenInclude(customer => customer.ApplicationUser)
                .ToListAsync();
            var newServiceOrder = await _unitOfWork.ServiceOrderRepository
                .GetAllQueryable()
                .Where(x => x.CreationDate >= DateTime.Now.AddDays(-30) && x.ServiceOrderStatus >= ServiceOrderStatus.Paid && x.ServiceOrderStatus != ServiceOrderStatus.Fail)
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
    }
}
