using Application;
using Application.Interfaces;
using Application.Repositories;
using Infrastructures.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

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
        private readonly IDeliveryFeeRepository _deliveryFeeRepository;
        private readonly IServiceRepository _serviceRepository;
        private readonly ITasksRepository _tasksRepository;
        private readonly IServiceOrderRepository _serviceOrderRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly IBaseTaskRepository _baseTaskRepository;
        private readonly IAnnualWorkingDayRepository _annualWorkingDayRepository;
        private readonly IServiceDayRepository _serviceDayRepository;
        private readonly IServiceImageRepository _serviceImageRepository;
        private readonly IOrderServiceTaskRepository _orderServiceTaskRepository;
        private readonly IDayInWeekRepository _dayInWeekRepository;
        private IDbContextTransaction _transaction;

        public UnitOfWork(AppDbContext dbContext, IGardenerRepository gardenerRepository, ICustomerRepository customerRepository,
            IProductRepository productRepository, ICategoryRepository categoryRepository, ISubCategoryRepository subcategoryRepository,
            IOrderRepository orderRepository, IOrderDetailRepository orderDetailRepository, IOrderTransactionRepository orderTransactionRepository,
            IProductImageRepository productImageRepository, ITagRepository tagRepository, IProductTagRepository productTagRepository,
            IDeliveryFeeRepository deliveryFeeRepository, IServiceRepository serviceRepository, ITasksRepository tasksRepository,
            IServiceOrderRepository serviceOrderRepository, IStaffRepository staffRepository, IBaseTaskRepository baseTaskRepository,
            IAnnualWorkingDayRepository annualWorkingDayRepository, IServiceDayRepository serviceDayRepository, IServiceImageRepository serviceImageRepository,
            IOrderServiceTaskRepository orderServiceTaskRepository, IDayInWeekRepository dayInWeekRepository)
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
            _deliveryFeeRepository = deliveryFeeRepository;
            _serviceRepository = serviceRepository;
            _tasksRepository = tasksRepository;
            _serviceOrderRepository = serviceOrderRepository;
            _staffRepository = staffRepository;
            _baseTaskRepository = baseTaskRepository;
            _annualWorkingDayRepository = annualWorkingDayRepository;
            _serviceDayRepository = serviceDayRepository;
            _serviceImageRepository = serviceImageRepository;
            _orderServiceTaskRepository = orderServiceTaskRepository;
            _dayInWeekRepository = dayInWeekRepository;
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

        public IDeliveryFeeRepository DeliveryFeeRepository => _deliveryFeeRepository;

        public IServiceRepository ServiceRepository => _serviceRepository;

        public ITasksRepository TasksRepository => _tasksRepository;

        public IServiceOrderRepository ServiceOrderRepository => _serviceOrderRepository;

        public IStaffRepository StaffRepository => _staffRepository;

        public IBaseTaskRepository BaseTaskRepository => _baseTaskRepository;

        public IAnnualWorkingDayRepository AnnualWorkingDayRepository => _annualWorkingDayRepository;

        public IServiceDayRepository ServiceDayRepository => _serviceDayRepository;

        public IServiceImageRepository ServiceImageRepository => _serviceImageRepository;

        public IOrderServiceTaskRepository OrderServiceTaskRepository => _orderServiceTaskRepository;

        public IDayInWeekRepository DayInWeekRepository => _dayInWeekRepository;
        public async Task<int> SaveChangeAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }
        public void BeginTransaction()
        {
            _transaction = _dbContext.Database.BeginTransaction();
        }
        public async Task CommitTransactionAsync()
        {
            try
            {
                await _dbContext.SaveChangesAsync();
                _transaction.Commit();
            }
            catch
            {
                _transaction.Rollback();
                throw;
            }
        }
        public void RollbackTransaction()
        {
            _transaction.Rollback();
        }
        public void ClearTrack()
        {
            _dbContext.ChangeTracker.Clear();
        }
    }
}
