using Application.Repositories;

namespace Application
{
    public interface IUnitOfWork
    {

        public IGardenerRepository GardenerRepository { get; }
        public ICustomerRepository CustomerRepository { get; }

        public Task<int> SaveChangeAsync();
    }
}
