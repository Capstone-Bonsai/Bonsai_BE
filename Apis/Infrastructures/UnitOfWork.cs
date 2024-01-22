using Application;
using Application.Repositories;

namespace Infrastructures
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _dbContext;
        private readonly IGardenerRepository _gardenerRepository;
        private readonly ICustomerRepository _customerRepository;

        public UnitOfWork(AppDbContext dbContext, IGardenerRepository gardenerRepository, ICustomerRepository customerRepository)
        {
            _dbContext = dbContext;
            _gardenerRepository = gardenerRepository;
            _customerRepository = customerRepository;
        }


        public IGardenerRepository GardenerRepository => _gardenerRepository;

        public ICustomerRepository CustomerRepository => _customerRepository;

        public async Task<int> SaveChangeAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }
    }
}
