using Application.Commons;
using Application.Services.Momo;
using Application.ViewModels.ServiceOrderViewModels;
using Application.ViewModels.TaskViewModels;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IServiceOrderService
    {
        Task CreateServiceOrder(ServiceOrderModel serviceOrderModel);
        Task UpdateServiceOrder(Guid serviceOrderId, ResponseServiceOrderModel responseServiceOrderModel);
        Task<Pagination<ServiceOrder>> GetServiceOrders(int pageIndex, int pageSize, bool isCustomer, Guid id);
        Task<List<ServiceOrderForGardenerViewModel>> GetWorkingCalendar(int month, int year, Guid id);
        Task<ServiceOrderForGardenerViewModel> GetServiceOrderByIdForGardener(Guid serviceOrderId);
        Task HandleIpnAsync(MomoRedirect momo);
        Task<string> PaymentContract(Guid contractId, string userId);
        Task<OverallServiceOrderViewModel> GetServiceOrderById(Guid serviceOrderId, bool isCustomer, Guid userId);
        Task AddContractImage(Guid contractId, ServiceOrderImageModel serviceOrderImageModel);
        Task UpdateServiceOrderStatus(Guid serviceOrderId, ServiceOrderStatus serviceOrderStatus);
    }
}
