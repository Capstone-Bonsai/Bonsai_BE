using Application.Interfaces;
using Application.ViewModels.ContractViewModels;
using AutoMapper;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class ContractService : IContractService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ContractService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<List<TaskViewModel>> GetTaskOfContract(Guid contractId)
        {
            List<TaskViewModel> tasks = new List<TaskViewModel>();
            var contract = await _unitOfWork.ContractRepository
                    .GetAllQueryable()
                    .AsNoTracking()
                    .Include(x => x.ServiceGarden)
                    .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == contractId);
            if (contract == null)
            {
                throw new Exception("Không tìm thấy hợp đồng!");
            }
            if (contract.ServiceType == Domain.Enums.ServiceType.BonsaiCare)
            {
                CustomerBonsai? customerBonsai = new CustomerBonsai();
                customerBonsai = await _unitOfWork.CustomerBonsaiRepository
                    .GetAllQueryable()
                    .AsNoTracking()
                    .Include(x => x.Bonsai)
                    .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == contract.CustomerBonsaiId);
                if (customerBonsai == null)
                {
                    throw new Exception("Không tìm thấy bonsai");
                }
                var careSteps = await _unitOfWork.CareStepRepository.GetAsync(isTakeAll: true, expression: x => !x.IsDeleted && x.CategoryId == customerBonsai.Bonsai.CategoryId);
                if (careSteps.Items.Count == 0)
                {
                    throw new Exception("Không tìm thấy bất cứ công việc nào của phân loại này");
                }
                foreach (CareStep careStep in careSteps.Items)
                {
                    tasks.Add(new TaskViewModel()
                    {
                        Id = careStep.Id,
                        Name = careStep.Step,
                    });
                }
                return tasks;
            }
            else
            {
                Service? service = new Service();
                service = await _unitOfWork.ServiceRepository
                    .GetAllQueryable()
                    .AsNoTracking()
                    .Include(x => x.ServiceBaseTasks)
                    .ThenInclude(x => x.BaseTask)
                    .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == contract.ServiceGarden.Id);
                if (service != null)
                {
                    var serviceBaseTasks = service.ServiceBaseTasks;
                    if (serviceBaseTasks != null && serviceBaseTasks.Any())
                    {
                        var baseTasks = serviceBaseTasks.Select(sb => sb.BaseTask).ToList();
                        foreach (BaseTask baseTask in baseTasks)
                        {
                            tasks.Add(new TaskViewModel()
                            {
                                Id = baseTask.Id,
                                Name = baseTask.Name,
                            });
                        }
                        return tasks;
                    }
                    else
                    {
                        throw new Exception("Không tìm thấy bất cứ công việc nào của phân loại này");
                    }
                }
                else
                {
                    throw new Exception("Không tìm thấy dịch vụ");
                }
            }
        }
        public async Task CreateContract(ContractModel contractModel)
        {

        }
    }
}
