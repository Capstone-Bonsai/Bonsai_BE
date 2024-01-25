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
        private readonly ISubCategoryRepository _subcategoryRepository;
        private readonly IProductImageRepository _productImageRepository;

        public UnitOfWork(AppDbContext dbContext, IGardenerRepository gardenerRepository, ICustomerRepository customerRepository, 
            IProductRepository productRepository, ICategoryRepository categoryRepository, ISubCategoryRepository subcategoryRepository,
            IProductImageRepository productImageRepository)
        {
            _dbContext = dbContext;
            _gardenerRepository = gardenerRepository;
            _customerRepository = customerRepository;
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _subcategoryRepository = subcategoryRepository;
            _productImageRepository = productImageRepository;
        }


        public IGardenerRepository GardenerRepository => _gardenerRepository;

        public ICustomerRepository CustomerRepository => _customerRepository;

        public IProductRepository ProductRepository => _productRepository;

        public ICategoryRepository CategoryRepository => _categoryRepository;

        public ISubCategoryRepository SubCategoryRepository => _subcategoryRepository;

        public IProductImageRepository ProductImageRepository => _productImageRepository;

        public async Task<int> SaveChangeAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }
    }
}
