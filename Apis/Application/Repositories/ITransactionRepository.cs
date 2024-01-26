using Application.ViewModels.OrderViewModels;

namespace Application.Repositories
{
    public interface ITransactionRepository
    {
        public Task CreateOrderByTransaction(OrderModel model, string? userId);
    }
}
