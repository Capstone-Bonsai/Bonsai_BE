using Application.ViewModels.OrderViewModels;

namespace Application.Interfaces
{
    public interface IOrderService
    {
        public Task<IList<string>> CreateOrderAsync(OrderModel model, string userId);
    }
}
