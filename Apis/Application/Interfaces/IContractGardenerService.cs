using Application.Commons;
using Application.ViewModels.ContractViewModels;
using Application.ViewModels.UserViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IContractGardenerService
    {
        Task<Pagination<UserViewModel>> GetGardenerOfContract(int pageIndex, int pageSize, Guid contractId);
        Task ChangeContractGardener(Guid contractId, ChangeGardenerViewModel changeGardenerViewModel);
        Task AddContractGardener(ContractGardenerModel contractGardenerModel);
    }
}
