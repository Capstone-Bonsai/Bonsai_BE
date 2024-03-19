using Application.Commons;
using Application.ViewModels.ContractViewModels;
using Application.ViewModels.TaskViewModels;
using Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IContractService
    {
        Task CreateContract(ContractModel contractModel);
        Task<Pagination<Contract>> GetContracts(int pageIndex, int pageSize);
    }
}
