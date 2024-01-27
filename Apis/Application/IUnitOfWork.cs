using Application.Interfaces;
using Application.Repositories;

namespace Application
{
    public interface IUnitOfWork
    {

        public IGardenerRepository GardenerRepository { get; }
        public ICustomerRepository CustomerRepository { get; }
        public IProductRepository ProductRepository { get; }
        public ICategoryRepository CategoryRepository { get; }
        public ISubCategoryRepository SubCategoryRepository { get; }
        public IProductImageRepository ProductImageRepository { get; }
        public IOrderRepository OrderRepository { get; }
        public IOrderDetailRepository OrderDetailRepository { get; }
        public IOrderTransactionRepository OrderTransactionRepository { get; }
        public ITagRepository TagRepository { get; }
        public IProductTagRepository ProductTagRepository { get; }
        public IDeliveryFeeRepository DeliveryFeeRepository { get; }

        public Task<int> SaveChangeAsync();
    }
}
