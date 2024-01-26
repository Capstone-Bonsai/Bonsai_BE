
﻿using Application.ViewModels.OrderViewModels;
﻿using Application.Services.Momo;
using Application.ViewModels.OrderViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IOrderService
    {
        public Task<IList<string>> ValidateOrderModel(OrderModel model, string userId);
        public Task<string> CreateOrderAsync(OrderModel model, string userId);
        public Task HandleIpnAsync(MomoRedirect momo);
        public Task<string> PaymentAsync(Guid tempId);
    }
}
