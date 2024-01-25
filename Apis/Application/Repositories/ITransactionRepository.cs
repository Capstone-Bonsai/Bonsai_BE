using Application.ViewModels.OrderViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Repositories
{
    public interface ITransactionRepository
    {
        public Task CreateOrderByTransaction(OrderModel model, string? userId);
    }
}
