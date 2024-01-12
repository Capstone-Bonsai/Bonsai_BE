using Application;
using Application.Repositories;

namespace Infrastructures
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _dbContext;
        private readonly IChemicalRepository _chemicalRepository;
        private readonly IUserRepository _userRepository;
        private readonly IGardenerRepository _gardenerRepository;
        private readonly ICustomerRepository _customerRepository;

        public UnitOfWork(AppDbContext dbContext,
            IChemicalRepository chemicalRepository,
            IUserRepository userRepository, IGardenerRepository gardenerRepository, ICustomerRepository customerRepository)
        {
            _dbContext = dbContext;
            _chemicalRepository = chemicalRepository;
            _userRepository = userRepository;
            _gardenerRepository = gardenerRepository;
            _customerRepository = customerRepository;
        }
        public IChemicalRepository ChemicalRepository => _chemicalRepository;

        public IUserRepository UserRepository => _userRepository;

        public IGardenerRepository GardenerRepository => _gardenerRepository;

        public ICustomerRepository CustomerRepository => _customerRepository;

        public async Task<int> SaveChangeAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }
    }
}
