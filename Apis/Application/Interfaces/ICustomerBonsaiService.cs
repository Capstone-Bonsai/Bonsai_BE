using Application.ViewModels.CustomerBonsaiViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ICustomerBonsaiService
    {
        Task AddBonsaiForCustomer(CustomerBonsaiModel customerBonsaiModel, Guid customerId);
    }
}
