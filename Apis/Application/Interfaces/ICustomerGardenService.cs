using Application.Commons;
using Application.ViewModels.CustomerGardenViewModels;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ICustomerGardenService
    {
        Task AddCustomerGarden(CustomerGardenModel customerGardenModel, Guid id);
        Task<Pagination<CustomerGarden>> GetByCustomerId(int pageIndex, int pageSize, Guid id);
        Task<Pagination<CustomerGarden>> GetAllByCustomerId(Guid id);
        Task Delete(Guid id);
        Task<Pagination<CustomerGarden>> GetPaginationForManager(int pageIndex, int pageSize);
        Task UpdateCustomerGarden(Guid customerGardenId, CustomerGardenModel customerGardenModel, Guid customerId);
        Task<CustomerGarden> GetById(Guid customerGardenId, bool isCustomer, Guid userId);
    }
}
