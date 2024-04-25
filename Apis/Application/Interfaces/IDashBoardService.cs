using Application.ViewModels.DashboardViewModels;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IDashBoardService
    {
        Task<DashboardViewModel> GetDashboardAsync();
        Task<List<RevenueLineGraph>> GetRevenueLineGraph(RevenueInputType revenueInputType);
    }
}
