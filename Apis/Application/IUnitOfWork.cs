using Application.Repositories;

namespace Application
{
    public interface IUnitOfWork
    {
        public IChemicalRepository ChemicalRepository { get; }

        public IUserRepository UserRepository { get; }
        public IGardenerRepository GardenerRepository { get; }
        public ICustomerRepository CustomerRepository { get; }

        public Task<int> SaveChangeAsync();
    }
}
