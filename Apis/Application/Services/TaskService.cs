﻿using Application.Interfaces;
using Application.ViewModels.TaskViewModels;
using AutoMapper;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class TaskService : ITaskService
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
                List<Expression<Func<BonsaiCareStep, object>>> includes = new List<Expression<Func<BonsaiCareStep, object>>>{
                                 x => x.CareStep
                                    };
                var bonsaiCareSteps = await _unitOfWork.BonsaiCareStepRepository.GetAsync(isTakeAll: true, expression: x => !x.IsDeleted && x.ContractId == contractId, includes: includes);
                if (bonsaiCareSteps.Items.Count == 0)
                {
                    throw new Exception("Không tìm thấy bất cứ công việc nào của phân loại này");
                }
                foreach (BonsaiCareStep bonsaiCareStep in bonsaiCareSteps.Items)
                {
                    tasks.Add(new TaskOfContract()
                    {
                        Id = bonsaiCareStep.Id,
                        Name = bonsaiCareStep.CareStep.Step,
                        CompletedTime = bonsaiCareStep.CompletedTime,
                        Note = bonsaiCareStep.Note,
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
                    .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == contract.ServiceGarden.ServiceId);
                if (service != null)
                {
                    List<Expression<Func<GardenCareTask, object>>> includes = new List<Expression<Func<GardenCareTask, object>>>{
                                 x => x.BaseTask
                                    };
                    var gardenCareTasks = await _unitOfWork.GardenCareTaskRepository.GetAsync(isTakeAll: true, expression: x => !x.IsDeleted && x.ContractId == contractId, includes: includes);
                    if (gardenCareTasks.Items.Count == 0)
                    {
                        throw new Exception("Không tìm thấy bất cứ công việc nào của phân loại này");
                    }
                    foreach (GardenCareTask gardenCareTask in gardenCareTasks.Items)
                    {
                        tasks.Add(new TaskOfContract()
                        {
                            Id = gardenCareTask.Id,
                            Name = gardenCareTask.BaseTask.Name,
                            CompletedTime = gardenCareTask.CompletedTime,
                            Note = gardenCareTask.Note,
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
                    throw new Exception("Không tìm thấy dịch vụ");
                }
            }
        }
        public async Task UpdateProgress(TaskModel taskModel)
        {
            var contract = await _unitOfWork.ContractRepository
                  .GetAllQueryable()
                  .AsNoTracking()
                  .Include(x => x.ServiceGarden)
                  .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == taskModel.ContractId);
            if (contract == null)
            {
                throw new Exception("Không tìm thấy hợp đồng!");
            }
            if (contract.ServiceType == Domain.Enums.ServiceType.BonsaiCare)
            {
                List<BonsaiCareStep> tasks = new List<BonsaiCareStep>();
                foreach (Guid id in taskModel.FinishedTasks)
                {
                    var task = await _unitOfWork.BonsaiCareStepRepository.GetByIdAsync(id);
                    if (task == null)
                    {
                        throw new Exception("Không tìm thấy công việc");
                    }
                    task.CompletedTime = DateTime.Now;
                    tasks.Add(task);
                }
                _unitOfWork.BonsaiCareStepRepository.UpdateRange(tasks);
                await _unitOfWork.SaveChangeAsync();
            }
            else
            {
                List<GardenCareTask> tasks = new List<GardenCareTask>();
                foreach (Guid id in taskModel.FinishedTasks)
                {
                    var task = await _unitOfWork.GardenCareTaskRepository.GetByIdAsync(id);
                    if (task == null)
                    {
                        throw new Exception("Không tìm thấy công việc");
                    }
                    task.CompletedTime = DateTime.Now;
                    tasks.Add(task);
                }
                _unitOfWork.GardenCareTaskRepository.UpdateRange(tasks);
                await _unitOfWork.SaveChangeAsync();
            }
        }
    }
}
