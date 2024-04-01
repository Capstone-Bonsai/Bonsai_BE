using Application.Commons;
using Application.ViewModels.CustomerBonsaiViewModels;
using Domain.Entities;
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
        Task CreateBonsai(Guid gardenId, BonsaiModelForCustomer bonsaiModelForCustomer);
        Task<Pagination<CustomerBonsai>> GetBonsaiOfGarden(Guid gardenId);
        Task<CustomerBonsai> GetCustomerBonsaiById(Guid customerBonsaiId);
    }
}
