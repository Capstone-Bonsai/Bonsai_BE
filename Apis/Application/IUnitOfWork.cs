﻿using Application.Interfaces;
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

        public Task<int> SaveChangeAsync();
        void BeginTransaction();
        Task CommitTransactionAsync();
        void RollbackTransaction();
        public void ClearTrack();
    }
}
