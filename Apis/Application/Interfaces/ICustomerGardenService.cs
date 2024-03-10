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
        Task<Pagination<CustomerGarden>> GetByCustomerId(Guid id);
        Task<Pagination<CustomerGarden>> Get();
    }
}
