using Application.Commons;
using Application.ViewModels.ServiceOrderViewModels;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IServiceOrderService
    {
        Task<Pagination<ServiceOrder>> GetServiceOrdersPagination(int pageIndex, int pageSize);
        Task<Pagination<ServiceOrder>> GetServiceOrders();
        Task AddServiceOrder(ServiceOrderModel serviceOrderModel, Guid customerId);
        Task ResponseServiceOrder(Guid staffId, Guid serviceOrderId, ResponseServiceOrderModel responseServiceOrderModel);
        Task<ServiceOrder?> GetOrderServiceById(Guid id);
        Task<string> PaymentAsync(Guid tempId);
    }
}
