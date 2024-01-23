using Application.Repositories;

namespace Application
{
    public interface IUnitOfWork
    {

        public IGardenerRepository GardenerRepository { get; }
        public ICustomerRepository CustomerRepository { get; }
        public IProductRepository ProductRepository { get; }
        public ICategoryRepository CategoryRepository { get; }
        public Task<int> SaveChangeAsync();
    }
}
