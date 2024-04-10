using Application.Commons;
using Application.Services.Momo;
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
        Task<Pagination<Contract>> GetContracts(int pageIndex, int pageSize, bool isCustomer, Guid id);
        Task<List<ContractViewModel>> GetWorkingCalendar(int month, int year, Guid id);
        Task<ContractViewModel> GetContractById(Guid id);
        Task HandleIpnAsync(MomoRedirect momo);
        Task<string> PaymentContract(Guid contractId, string userId);
        Task<List<ContractViewModel>> GetTodayProject(Guid id);
    }
}
