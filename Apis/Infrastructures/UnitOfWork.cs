using Application;
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
        private readonly ICustomerGardenRepository _customerGardenRepository;
        private readonly ICustomerGardenImageRepository _customerGardenImageRepository;
        private readonly ICustomerBonsaiRepository _customerBonsaiRepository;
        private readonly ICareStepRepository _careStepRepository;
        private readonly IServiceRepository _serviceRepository;
        private readonly IBaseTaskRepository _baseTaskRepository;
        private readonly IServiceBaseTaskRepository _serviceBaseTaskRepository;
        private readonly IServiceOrderRepository _serviceOrderRepository;
        private readonly IGardenCareTaskRepository _gardenCareTaskRepository;
        private readonly IBonsaiCareStepRepository _bonsaiCareStepRepository;
        private readonly IServiceOrderTransactionRepository _serviceOrderTransactionRepository;
        private readonly IComplaintRepository _complaintRepository;
        private readonly IComplaintImageRepository _complaintImageRepository;
        private readonly IContractRepository _contractRepository;
        private readonly IDeliveryImageRepository _deliveryImageRepository;
        private readonly IServiceTypeRepository _serviceTypeRepository;
        private readonly IServiceOrderGardenerRepository _serviceOrderGardenerRepository;
        private IDbContextTransaction _transaction;

        public UnitOfWork(AppDbContext dbContext, IGardenerRepository gardenerRepository, ICustomerRepository customerRepository,
            IBonsaiRepository bonsaiRepository, ICategoryRepository categoryRepository, IOrderRepository orderRepository, 
            IOrderDetailRepository orderDetailRepository, IOrderTransactionRepository orderTransactionRepository,  IBonsaiImageRepository bonsaiImageRepository,
            IDeliveryFeeRepository deliveryFeeRepository, IStyleRepository styleRepository, IStaffRepository staffRepository, ICustomerGardenRepository customerGardenRepository,
            ICustomerGardenImageRepository customerGardenImageRepository, ICustomerBonsaiRepository customerBonsaiRepository, ICareStepRepository careStepRepository,
            IServiceRepository serviceRepository,IBaseTaskRepository baseTaskRepository, IServiceBaseTaskRepository serviceBaseTaskRepository,
           IServiceOrderRepository serviceOrderRepository, IGardenCareTaskRepository gardenCareTaskRepository,
            IBonsaiCareStepRepository bonsaiCareStepRepository,
             IServiceOrderTransactionRepository serviceOrderTransactionRepository, IComplaintRepository complaintRepository, 
             IComplaintImageRepository complaintImageRepository, IContractRepository contractRepository, IDeliveryImageRepository deliveryImageRepository,
             IServiceTypeRepository serviceTypeRepository, IServiceOrderGardenerRepository serviceOrderGardenerRepository
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
            _customerGardenRepository = customerGardenRepository;
            _customerGardenImageRepository = customerGardenImageRepository;
            _customerBonsaiRepository = customerBonsaiRepository;
            _careStepRepository = careStepRepository;
            _serviceRepository = serviceRepository;
            _baseTaskRepository = baseTaskRepository;
            _serviceBaseTaskRepository = serviceBaseTaskRepository;
            _serviceOrderRepository = serviceOrderRepository;
            _gardenCareTaskRepository = gardenCareTaskRepository;
            _bonsaiCareStepRepository = bonsaiCareStepRepository;
            _serviceOrderTransactionRepository = serviceOrderTransactionRepository;
            _complaintRepository = complaintRepository;
            _complaintImageRepository = complaintImageRepository;
            _contractRepository = contractRepository;
            _deliveryImageRepository = deliveryImageRepository;
            _serviceTypeRepository = serviceTypeRepository;
            _serviceOrderGardenerRepository = serviceOrderGardenerRepository;
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

        public ICustomerGardenRepository CustomerGardenRepository => _customerGardenRepository;

        public ICustomerGardenImageRepository CustomerGardenImageRepository => _customerGardenImageRepository;

        public ICustomerBonsaiRepository CustomerBonsaiRepository => _customerBonsaiRepository;

        public ICareStepRepository CareStepRepository => _careStepRepository;

        public IServiceRepository ServiceRepository => _serviceRepository;

        public IBaseTaskRepository BaseTaskRepository => _baseTaskRepository;

        public IServiceBaseTaskRepository ServiceBaseTaskRepository => _serviceBaseTaskRepository;

        public IServiceOrderRepository ServiceOrderRepository => _serviceOrderRepository;

        public IGardenCareTaskRepository GardenCareTaskRepository => _gardenCareTaskRepository;

        public IBonsaiCareStepRepository BonsaiCareStepRepository => _bonsaiCareStepRepository;

        public IServiceOrderTransactionRepository ServiceOrderTransactionRepository => _serviceOrderTransactionRepository;

        public IComplaintImageRepository ComplaintImageRepository => _complaintImageRepository;

        public IContractRepository ContractRepository => _contractRepository;

        public IDeliveryImageRepository DeliveryImageRepository => _deliveryImageRepository;

        public IComplaintRepository ComplaintRepository => _complaintRepository;

        public IServiceTypeRepository ServiceTypeRepository => _serviceTypeRepository;

        public IServiceOrderGardenerRepository ServiceOrderGardenerRepository => _serviceOrderGardenerRepository;

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
