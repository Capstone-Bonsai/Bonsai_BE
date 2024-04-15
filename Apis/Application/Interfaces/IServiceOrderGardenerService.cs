using Application.Commons;
using Application.ViewModels.ServiceOrderViewModels;
using Application.ViewModels.UserViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IServiceOrderGardenerService
    {
        Task<Pagination<UserViewModel>> GetGardenerOfServiceOrder(int pageIndex, int pageSize, Guid contractId);
        Task AddGardener(ServiceOrderGardenerModel serviceOrderGardenerModel);
    }
}
