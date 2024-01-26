using Application.ViewModels.OrderViewModels;

namespace Application.Repositories
{
    public interface ITransactionRepository
    {
        public Task<Guid> CreateOrderByTransaction(OrderModel model, string? userId);
    }
}
