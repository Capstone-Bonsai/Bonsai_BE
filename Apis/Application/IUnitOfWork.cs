using Application.Repositories;

namespace Application
{
    public interface IUnitOfWork
    {
        public IGardenerRepository GardenerRepository { get; }
        public ICustomerRepository CustomerRepository { get; }
        public IBonsaiRepository BonsaiRepository { get; }
        public ICategoryRepository CategoryRepository { get; }
        public IBonsaiImageRepository BonsaiImageRepository { get; }
        public IOrderRepository OrderRepository { get; }
        public IOrderDetailRepository OrderDetailRepository { get; }
        public IOrderTransactionRepository OrderTransactionRepository { get; }
        public IDeliveryFeeRepository DeliveryFeeRepository { get; }
        public IStaffRepository StaffRepository { get; }
        public IStyleRepository StyleRepository { get; }
        public ICustomerGardenRepository CustomerGardenRepository { get; }
        public ICustomerGardenImageRepository CustomerGardenImageRepository { get; }
        public ICustomerBonsaiRepository CustomerBonsaiRepository { get; }
        public ICareStepRepository CareStepRepository { get; }
        public IServiceRepository ServiceRepository { get; }
        public IBaseTaskRepository BaseTaskRepository { get; }
        public IServiceBaseTaskRepository ServiceBaseTaskRepository { get; }
        public IServiceGardenRepository ServiceGardenRepository { get; }
        public IContractRepository ContractRepository { get; }
        public IGardenCareTaskRepository GardenCareTaskRepository { get; }
        public IBonsaiCareStepRepository BonsaiCareStepRepository { get; }
        public IServiceSurchargeRepository ServiceSurchargeRepository { get; }
        public IContractGardenerRepository ContractGardenerRepository { get; }
        public Task<int> SaveChangeAsync();
        void BeginTransaction();
        Task CommitTransactionAsync();
        void RollbackTransaction();
        public void ClearTrack();
    }
}
