
﻿using Application.ViewModels.OrderViewModels;
﻿using Application.Services.Momo;
using Application.ViewModels.OrderViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Commons;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IOrderService
    {
        public Task<IList<string>> ValidateOrderModel(OrderModel model, string userId);
        public Task<string> CreateOrderAsync(OrderModel model, string userId);
        public Task HandleIpnAsync(MomoRedirect momo);
        public Task<string> PaymentAsync(Guid tempId);
        public Task<Pagination<Order>> GetPaginationAsync(string userId, int pageIndex = 0, int pageSize = 10);
        public  Task<Order> GetByIdAsync(string userId, Guid orderId);
    }
}
