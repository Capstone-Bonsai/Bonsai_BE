using Application;
using Application.Repositories;

namespace Infrastructures
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _dbContext;
        private readonly IGardenerRepository _gardenerRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public UnitOfWork(AppDbContext dbContext, IGardenerRepository gardenerRepository, ICustomerRepository customerRepository, IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _dbContext = dbContext;
            _gardenerRepository = gardenerRepository;
            _customerRepository = customerRepository;
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }


        public IGardenerRepository GardenerRepository => _gardenerRepository;

        public ICustomerRepository CustomerRepository => _customerRepository;

        public IProductRepository ProductRepository => _productRepository;

        public ICategoryRepository CategoryRepository => _categoryRepository;

        public async Task<int> SaveChangeAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }
    }
}
