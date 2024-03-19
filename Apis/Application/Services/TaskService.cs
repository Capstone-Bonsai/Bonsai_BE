using Application.ViewModels.TaskViewModels;
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
    public class TaskService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public TaskService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<TaskViewModel> GetTasksOfContract(Guid contractId)
        {
            List<TaskOfContract> tasks = new List<TaskOfContract>();
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
                    tasks.Add(new TaskOfContract()
                    {
                        Id = careStep.Id,
                        Name = careStep.Step,
                    });
                }
                TaskViewModel taskViewModel = new TaskViewModel()
                {
                    ContractId = contractId,
                    Address = contract.Address,
                    CustomerName = contract.CustomerName,
                    PhoneNumber = contract.CustomerPhoneNumber,
                    TaskOfContracts = tasks
                };
                return taskViewModel;
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
                            tasks.Add(new TaskOfContract()
                            {
                                Id = baseTask.Id,
                                Name = baseTask.Name,
                            });
                        }
                        TaskViewModel taskViewModel = new TaskViewModel()
                        {
                            ContractId = contractId,
                            Address = contract.Address,
                            CustomerName = contract.CustomerName,
                            PhoneNumber = contract.CustomerPhoneNumber,
                            TaskOfContracts = tasks
                        };
                        return taskViewModel;
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
    }
}
