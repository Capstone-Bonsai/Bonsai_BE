using Application.ViewModels.DashboardViewModels;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IRevenueService
    {
        Task<RevenueViewModel> GetRevenueAsync();
        Task<byte[]> GetExcel();
        Task<List<Order>> GetOrdersAsync();
        Task<List<ServiceOrder>> GetServiceOrdersAsync();
    }
}
