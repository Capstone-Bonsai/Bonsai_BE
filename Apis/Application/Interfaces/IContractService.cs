using Application.ViewModels.ContractViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IContractService
    {
        Task<List<TaskViewModel>> GetTaskOfContract(Guid contractId);
    }
}
