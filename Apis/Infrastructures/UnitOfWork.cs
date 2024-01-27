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
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderDetailRepository _orderDetailRepository;
        private readonly IOrderTransactionRepository _orderTransactionRepository;
        private readonly ITagRepository _tagRepository;
        private readonly IProductTagRepository _productTagRepository;

        public UnitOfWork(AppDbContext dbContext, IGardenerRepository gardenerRepository, ICustomerRepository customerRepository,
            IProductRepository productRepository, ICategoryRepository categoryRepository, ISubCategoryRepository subcategoryRepository,
            IOrderRepository orderRepository, IOrderDetailRepository orderDetailRepository, IOrderTransactionRepository orderTransactionRepository,
            IProductImageRepository productImageRepository, ITagRepository tagRepository, IProductTagRepository productTagRepository)
        {
            _dbContext = dbContext;
            _gardenerRepository = gardenerRepository;
            _customerRepository = customerRepository;
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _subcategoryRepository = subcategoryRepository;
            _productImageRepository = productImageRepository;
            _orderRepository = orderRepository;
            _orderDetailRepository = orderDetailRepository;
            _orderTransactionRepository = orderTransactionRepository;
            _tagRepository = tagRepository;
            _productTagRepository = productTagRepository;
        }


        public IGardenerRepository GardenerRepository => _gardenerRepository;

        public ICustomerRepository CustomerRepository => _customerRepository;

        public IProductRepository ProductRepository => _productRepository;

        public ICategoryRepository CategoryRepository => _categoryRepository;

        public ISubCategoryRepository SubCategoryRepository => _subcategoryRepository;

        public IProductImageRepository ProductImageRepository => _productImageRepository;

        public IOrderRepository OrderRepository => _orderRepository;

        public IOrderDetailRepository OrderDetailRepository => _orderDetailRepository;

        public IOrderTransactionRepository OrderTransactionRepository => _orderTransactionRepository;

        public ITagRepository TagRepository => _tagRepository;
        public IProductTagRepository ProductTagRepository => _productTagRepository;



        public async Task<int> SaveChangeAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }
    }
}
