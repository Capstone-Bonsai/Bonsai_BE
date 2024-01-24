using Application.Interfaces;
using Application.Validations.Order;
using Application.ViewModels.OrderViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class OrderService:IOrderService
    {
        public OrderService() { }
        public async Task<IList<string>> ValidateOrderModel(OrderModel model)
        {
            var validator = new OrderModelValidator();
            var result = await validator.ValidateAsync(model);
            if (!result.IsValid)
            {
                var errors = new List<string>();
                errors.AddRange(result.Errors.Select(x => x.ErrorMessage));
                return errors;
            }
            return null; 
        }
    }
}
