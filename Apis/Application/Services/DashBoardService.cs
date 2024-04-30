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

            var newUser = await _unitOfWork.CustomerRepository.GetAsync(isTakeAll: true, expression: x => x.CreationDate >= DateTime.Now.AddDays(-30) && x.ApplicationUser.IsRegister);
            var newOrder = await _unitOfWork.OrderRepository.GetAsync(isTakeAll: true, expression: x => x.CreationDate >= DateTime.Now.AddDays(-30) && x.OrderStatus >= OrderStatus.Paid && x.OrderStatus != Domain.Enums.OrderStatus.Failed);
            double totalOrderIncome = newOrder.Items.Sum(item => item.TotalPrice);
            var newServiceOrder = await _unitOfWork.ServiceOrderRepository.GetAsync(isTakeAll: true, expression: x => x.CreationDate >= DateTime.Now.AddDays(-30) && x.ServiceOrderStatus >= Domain.Enums.ServiceOrderStatus.Paid && x.ServiceOrderStatus != ServiceOrderStatus.Fail && x.ServiceOrderStatus != ServiceOrderStatus.Canceled);
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
                    .Where(x => x.Bonsai.CategoryId == category.Id && x.Order.OrderStatus >= OrderStatus.Paid && x.Order.OrderStatus != Domain.Enums.OrderStatus.Failed && x.CreationDate >= DateTime.Now.AddDays(-30))
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
                                 x.OrderStatus >= OrderStatus.Paid && x.OrderStatus != OrderStatus.Failed
            );

            var serviceOrders = await _unitOfWork.ServiceOrderRepository.GetAsync(
                isTakeAll: true,
                expression: x => x.CreationDate >= DateTime.Now.Date.AddMonths(-11) && x.CreationDate < DateTime.Now.Date &&
                                 x.ServiceOrderStatus >= ServiceOrderStatus.Paid && x.ServiceOrderStatus != ServiceOrderStatus.Fail && x.ServiceOrderStatus != ServiceOrderStatus.Canceled
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
        
        public async Task<DashboardViewModel> GetDashboardForStaffAsync()
        {

            var newUser = await _unitOfWork.CustomerRepository.GetAsync(isTakeAll: true, expression: x => x.CreationDate >= DateTime.Now.AddDays(-30) && x.ApplicationUser.IsRegister);
            var newOrder = await _unitOfWork.OrderRepository.GetAsync(isTakeAll: true, expression: x => x.CreationDate >= DateTime.Now.AddDays(-30) && x.OrderStatus >= Domain.Enums.OrderStatus.Paid && x.OrderStatus != Domain.Enums.OrderStatus.Failed);
            var newServiceOrder = await _unitOfWork.ServiceOrderRepository.GetAsync(isTakeAll: true, expression: x => x.CreationDate >= DateTime.Now.AddDays(-30) && x.ServiceOrderStatus >= Domain.Enums.ServiceOrderStatus.Paid && x.ServiceOrderStatus != Domain.Enums.ServiceOrderStatus.Fail);
            var currentServiceOrder = await _unitOfWork.ServiceOrderRepository.GetAsync(isTakeAll: true, expression: x => x.CreationDate >= DateTime.Now.AddDays(-30) && x.ServiceOrderStatus >= Domain.Enums.ServiceOrderStatus.Paid && x.ServiceOrderStatus != Domain.Enums.ServiceOrderStatus.Completed && x.ServiceOrderStatus != Domain.Enums.ServiceOrderStatus.Fail);
            DashboardViewModel dashboardViewModel = new DashboardViewModel()
            {
                NewUser = newUser.Items.Count,
                NewOrder = newOrder.Items.Count,
                CurrentServiceOngoing = currentServiceOrder.Items.Count
            };
            return dashboardViewModel;
        }
    }
}
