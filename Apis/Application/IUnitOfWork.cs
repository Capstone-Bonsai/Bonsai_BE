using Application.Interfaces;
using Application.Repositories;

namespace Application
{
    public interface IUnitOfWork
    {
        public IGardenerRepository GardenerRepository { get; }
        public ICustomerRepository CustomerRepository { get; }
        public IProductRepository ProductRepository { get; }
        public ICategoryRepository CategoryRepository { get; }
        public ISubCategoryRepository SubCategoryRepository { get; }
        public IProductImageRepository ProductImageRepository { get; }
        public IOrderRepository OrderRepository { get; }
        public IOrderDetailRepository OrderDetailRepository { get; }
        public IOrderTransactionRepository OrderTransactionRepository { get; }
        public ITagRepository TagRepository { get; }
        public IProductTagRepository ProductTagRepository { get; }
        public IDeliveryFeeRepository DeliveryFeeRepository { get; }
        public IServiceRepository ServiceRepository { get; }
        public ITasksRepository TasksRepository { get; }
        public IServiceOrderRepository ServiceOrderRepository { get; }
        public IStaffRepository StaffRepository { get; }
        public IBaseTaskRepository BaseTaskRepository { get; }
        public IAnnualWorkingDayRepository AnnualWorkingDayRepository { get; }
        public IServiceDayRepository ServiceDayRepository { get; }
        public IServiceImageRepository ServiceImageRepository { get; }
        public IOrderServiceTaskRepository OrderServiceTaskRepository { get; }
        public Task<int> SaveChangeAsync();
        void BeginTransaction();
        Task CommitTransactionAsync();
        void RollbackTransaction();
        public void ClearTrack();
    }
}
