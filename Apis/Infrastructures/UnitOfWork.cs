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
        private readonly IBonsaiRepository _bonsaiRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IBonsaiImageRepository _bonsaiImageRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderDetailRepository _orderDetailRepository;
        private readonly IOrderTransactionRepository _orderTransactionRepository;
        private readonly IDeliveryFeeRepository _deliveryFeeRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly IStyleRepository _styleRepository;
        private IDbContextTransaction _transaction;

        public UnitOfWork(AppDbContext dbContext, IGardenerRepository gardenerRepository, ICustomerRepository customerRepository,
            IBonsaiRepository bonsaiRepository, ICategoryRepository categoryRepository, IOrderRepository orderRepository, 
            IOrderDetailRepository orderDetailRepository, IOrderTransactionRepository orderTransactionRepository,  IBonsaiImageRepository bonsaiImageRepository,
            IDeliveryFeeRepository deliveryFeeRepository, IStyleRepository styleRepository, IStaffRepository staffRepository
         )
        {
            _dbContext = dbContext;
            _gardenerRepository = gardenerRepository;
            _customerRepository = customerRepository;
            _bonsaiRepository = bonsaiRepository;
            _categoryRepository = categoryRepository;
            _bonsaiImageRepository = bonsaiImageRepository;
            _orderRepository = orderRepository;
            _orderDetailRepository = orderDetailRepository;
            _orderTransactionRepository = orderTransactionRepository;
            _deliveryFeeRepository = deliveryFeeRepository;
            _styleRepository = styleRepository;
            _staffRepository = staffRepository;

        }


        public IGardenerRepository GardenerRepository => _gardenerRepository;

        public ICustomerRepository CustomerRepository => _customerRepository;

        public IBonsaiRepository BonsaiRepository => _bonsaiRepository;

        public ICategoryRepository CategoryRepository => _categoryRepository;

        public IStyleRepository StyleRepository => _styleRepository;

        public IBonsaiImageRepository BonsaiImageRepository => _bonsaiImageRepository;

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
