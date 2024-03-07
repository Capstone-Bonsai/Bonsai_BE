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
        private readonly IBonsaiRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IBonsaiImageRepository _productImageRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderDetailRepository _orderDetailRepository;
        private readonly IOrderTransactionRepository _orderTransactionRepository;
        private readonly IDeliveryFeeRepository _deliveryFeeRepository;
        private readonly IStaffRepository _staffRepository;
        private IDbContextTransaction _transaction;

        public UnitOfWork(AppDbContext dbContext, IGardenerRepository gardenerRepository, ICustomerRepository customerRepository,
            IBonsaiRepository productRepository, ICategoryRepository categoryRepository,
            IOrderRepository orderRepository, IOrderDetailRepository orderDetailRepository, IOrderTransactionRepository orderTransactionRepository,
            IBonsaiImageRepository productImageRepository,
            IDeliveryFeeRepository deliveryFeeRepository,
         IStaffRepository staffRepository
         )
        {
            _dbContext = dbContext;
            _gardenerRepository = gardenerRepository;
            _customerRepository = customerRepository;
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _productImageRepository = productImageRepository;
            _orderRepository = orderRepository;
            _orderDetailRepository = orderDetailRepository;
            _orderTransactionRepository = orderTransactionRepository;

            _deliveryFeeRepository = deliveryFeeRepository;

            _staffRepository = staffRepository;

        }


        public IGardenerRepository GardenerRepository => _gardenerRepository;

        public ICustomerRepository CustomerRepository => _customerRepository;

        public IBonsaiRepository ProductRepository => _productRepository;

        public ICategoryRepository CategoryRepository => _categoryRepository;



        public IBonsaiImageRepository ProductImageRepository => _productImageRepository;

        public IOrderRepository OrderRepository => _orderRepository;

        public IOrderDetailRepository OrderDetailRepository => _orderDetailRepository;

        public IOrderTransactionRepository OrderTransactionRepository => _orderTransactionRepository;



        public IDeliveryFeeRepository DeliveryFeeRepository => _deliveryFeeRepository;



        public IStaffRepository StaffRepository => _staffRepository;


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
