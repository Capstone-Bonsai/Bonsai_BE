using Application.Interfaces;
using Application.Utils;
using Application.ViewModels.DashboardViewModels;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
            var newOrder = await _unitOfWork.OrderRepository.GetAsync(isTakeAll: true, expression: x => x.CreationDate >= DateTime.Now.AddDays(-30) && x.OrderStatus >= Domain.Enums.OrderStatus.Paid);
            double totalOrderIncome = newOrder.Items.Sum(item => item.Price);
            var newServiceOrder = await _unitOfWork.ServiceOrderRepository.GetAsync(isTakeAll: true, expression: x => x.CreationDate >= DateTime.Now.AddDays(-30) && x.ServiceOrderStatus >= Domain.Enums.ServiceOrderStatus.Paid);
            double totalServiceOrderIncome = newServiceOrder.Items.Sum(item => item.TotalPrice);
            var currentServiceOrder = await _unitOfWork.ServiceOrderRepository.GetAsync(isTakeAll: true, expression: x => x.CreationDate >= DateTime.Now.AddDays(-30) && x.ServiceOrderStatus >= Domain.Enums.ServiceOrderStatus.Paid && x.ServiceOrderStatus != Domain.Enums.ServiceOrderStatus.Completed);
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
            foreach(Category category in categories.Items)
            {
                var orderDetail = await _unitOfWork.OrderDetailRepository.GetAllQueryable()
                    .AsNoTracking()
                    .Where(x => x.Bonsai.CategoryId == category.Id && x.Order.OrderStatus >= Domain.Enums.OrderStatus.Paid && x.CreationDate >= DateTime.Now.AddDays(-30))
                    .ToListAsync();
                double totalCategoryOrderDetailPrice = orderDetail.Sum(x => x.Price);
                double percent = (totalCategoryOrderDetailPrice / totalOrderIncome) * 100;
                orderCircleGraphs.Add(new OrderCircleGraph()
                {
                    CategoryName = category.Name,
                    Percent = percent
                });
            }
            return orderCircleGraphs;
        }
        private async Task<List<ServiceOrderCircleGraph>> GetServiceOrderCircleGraph(double totalServiceOrderIncome)
        {
            if (totalServiceOrderIncome == 0) return new List<ServiceOrderCircleGraph>();
            List<ServiceOrderCircleGraph> serviceOrderCircleGraphs = new List<ServiceOrderCircleGraph>();
            var services = await _unitOfWork.ServiceRepository.GetAsync(isTakeAll: true, expression: x => !x.IsDeleted);
            foreach (Service service in services.Items)
            {
                var serviceOrder = await _unitOfWork.ServiceOrderRepository.GetAllQueryable()
                    .AsNoTracking()
                    .Where(x => x.Service.Id == service.Id && x.ServiceOrderStatus >= Domain.Enums.ServiceOrderStatus.Paid && x.CreationDate >= DateTime.Now.AddDays(-30))
                    .ToListAsync();
                double totalCategoryOrderDetailPrice = serviceOrder.Sum(x => x.TotalPrice);
                double percent = (totalCategoryOrderDetailPrice / totalServiceOrderIncome) * 100;
                serviceOrderCircleGraphs.Add(new ServiceOrderCircleGraph()
                {
                    ServiceName = service.Name,
                    Percent = percent
                });
            }
            return serviceOrderCircleGraphs;
        }
    }
}
