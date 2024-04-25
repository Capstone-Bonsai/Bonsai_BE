
﻿using Application.ViewModels.OrderViewModels;
﻿using Application.Services.Momo;
using Application.Commons;
using Domain.Entities;
using Domain.Enums;

namespace Application.Interfaces
{
    public interface IOrderService
    {
        public Task<IList<string>> ValidateOrderModel(OrderModel model, string userId);
        public Task<string> CreateOrderAsync(OrderModel model, string userId);
        public Task HandleIpnAsync(MomoRedirect momo);
        public Task<string> PaymentAsync(Guid tempId);
        public Task<Pagination<OrderViewModel>> GetPaginationAsync(string userId, int pageIndex = 0, int pageSize = 10);
        public  Task<Order> GetByIdAsync(string userId, Guid orderId);
        public Task UpdateOrderStatusAsync(Guid orderId, OrderStatus orderStatus);
        Task FinishDeliveryOrder(Guid orderId, FinishDeliveryOrderModel finishDeliveryOrderModel);
        Task AddGardenerForOrder(Guid orderId, Guid gardenerId);
        Task CreateNotificationForStaff(Guid userId, Guid orderId);
    }
}
