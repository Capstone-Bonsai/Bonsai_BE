using Application.Interfaces;
using Application.Utils;
using Application.ViewModels.TaskViewModels;
using AutoMapper;
using Domain.Entities;
using Firebase.Auth;
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
        private readonly IdUtil _idUtil;
        private readonly INotificationService _notificationService;

        public TaskService(IUnitOfWork unitOfWork, IMapper mapper, IdUtil idUtil, INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _idUtil = idUtil;
            _notificationService = notificationService;
        }
       public async Task<TaskViewModel> GetTasksOfServiceOrder(Guid serviceOrderId)
        {
            List<TaskOfServiceOrder> tasks = new List<TaskOfServiceOrder>();
            var serviceOrder = await _unitOfWork.ServiceOrderRepository
                    .GetAllQueryable()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == serviceOrderId);
            if (serviceOrder == null)
            {
                throw new Exception("Không tìm thấy đơn đặt hàng dịch vụ!");
            }
            if (serviceOrder.CustomerBonsaiId != null)
            {
                CustomerBonsai? customerBonsai = new CustomerBonsai();
                customerBonsai = await _unitOfWork.CustomerBonsaiRepository
                    .GetAllQueryable()
                    .AsNoTracking()
                    .Include(x => x.Bonsai)
                    .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == serviceOrder.CustomerBonsaiId);
                if (customerBonsai == null)
                {
                    throw new Exception("Không tìm thấy bonsai");
                }
                List<Expression<Func<BonsaiCareStep, object>>> includes = new List<Expression<Func<BonsaiCareStep, object>>>{
                                 x => x.CareStep
                                    };
                var bonsaiCareSteps = await _unitOfWork.BonsaiCareStepRepository.GetAsync(isTakeAll: true, expression: x => !x.IsDeleted && x.ServiceOrderId == serviceOrderId, includes: includes);
                if (bonsaiCareSteps.Items.Count == 0)
                {
                    throw new Exception("Không tìm thấy bất cứ công việc nào của phân loại này");
                }
                foreach (BonsaiCareStep bonsaiCareStep in bonsaiCareSteps.Items)
                {
                    tasks.Add(new TaskOfServiceOrder()
                    {
                        Id = bonsaiCareStep.Id,
                        Name = bonsaiCareStep.CareStep.Step,
                        CompletedTime = bonsaiCareStep.CompletedTime,
                        Note = bonsaiCareStep.Note,
                    });
                }
                TaskViewModel taskViewModel = new TaskViewModel()
                {
                    ServiceOrderId = serviceOrderId,
                    Address = serviceOrder.Address,
                    CustomerName = serviceOrder.CustomerName,
                    PhoneNumber = serviceOrder.CustomerPhoneNumber,
                    TaskOfServiceOrders = tasks
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
                    .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == serviceOrder.ServiceId);
                if (service != null)
                {
                    List<Expression<Func<GardenCareTask, object>>> includes = new List<Expression<Func<GardenCareTask, object>>>{
                                 x => x.BaseTask
                                    };
                    var gardenCareTasks = await _unitOfWork.GardenCareTaskRepository.GetAsync(isTakeAll: true, expression: x => !x.IsDeleted && x.ServiceOrderId == serviceOrderId, includes: includes);
                    if (gardenCareTasks.Items.Count == 0)
                    {
                        throw new Exception("Không tìm thấy bất cứ công việc nào của phân loại này");
                    }
                    foreach (GardenCareTask gardenCareTask in gardenCareTasks.Items)
                    {
                        tasks.Add(new TaskOfServiceOrder()
                        {
                            Id = gardenCareTask.Id,
                            Name = gardenCareTask.BaseTask.Name,
                            CompletedTime = gardenCareTask.CompletedTime,
                            Note = gardenCareTask.Note,
                        });
                    }
                    TaskViewModel taskViewModel = new TaskViewModel()
                    {
                        ServiceOrderId = serviceOrderId,
                        Address = serviceOrder.Address,
                        CustomerName = serviceOrder.CustomerName,
                        PhoneNumber = serviceOrder.CustomerPhoneNumber,
                        TaskOfServiceOrders = tasks
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
            if (taskModel.FinishedTasks == null || taskModel.FinishedTasks.Count == 0)
            {
                throw new Exception("Chưa chọn công việc nào");
            }
            var serviceOrder = await _unitOfWork.ServiceOrderRepository
                  .GetAllQueryable()
                  .AsNoTracking()
                  .Include(x => x.CustomerGarden.Customer.ApplicationUser)
                  .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == taskModel.ServiceOrderId);
            if (serviceOrder == null)
            {
                throw new Exception("Không tìm thấy đơn đặt hàng dịch vụ!");
            }
            if (serviceOrder.CustomerBonsaiId != null)
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
                var unfinishedTask = await _unitOfWork.BonsaiCareStepRepository.GetAsync(isTakeAll: true, expression: x => x.ServiceOrderId == serviceOrder.Id && x.CompletedTime == null);
                if (unfinishedTask.Items.Count == 0)
                {
                    if (serviceOrder.ServiceOrderStatus == Domain.Enums.ServiceOrderStatus.ProcessingComplaint)
                    {
                        
                        serviceOrder.ServiceOrderStatus = Domain.Enums.ServiceOrderStatus.DoneTaskComplaint;
                          var complaint = await _unitOfWork.ComplaintRepository.GetAsync(isTakeAll: true, expression: x => x.ServiceOrderId == serviceOrder.Id && x.ComplaintStatus == Domain.Enums.ComplaintStatus.Processing);
                        if (complaint.Items.Count != 0) {
                            complaint.Items[0].ComplaintStatus = Domain.Enums.ComplaintStatus.Completed;
                            _unitOfWork.ComplaintRepository.Update(complaint.Items[0]);
                        }
                    }
                    else
                    {
                        serviceOrder.ServiceOrderStatus = Domain.Enums.ServiceOrderStatus.TaskFinished;
                    } 
                    _unitOfWork.ServiceOrderRepository.Update(serviceOrder);
                    var userId = await _idUtil.GetApplicationUserId(serviceOrder.CustomerGarden.CustomerId);
                    await _notificationService.SendMessageForUserId(userId, "Hoàn thành công việc", $"Đơn đặt hàng dịch vụ tại {serviceOrder.CustomerGarden.Address} đã hoàn thành công việc!");
                    await _notificationService.SendToStaff("Hoàn thành công việc", $"Đơn đặt hàng dịch vụ cho {serviceOrder.CustomerGarden.Customer.ApplicationUser.Email} đã hoàn thành công việc!");
                }
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
                var unfinishedTask = await _unitOfWork.GardenCareTaskRepository.GetAsync(isTakeAll: true, expression: x => x.ServiceOrderId == serviceOrder.Id && x.CompletedTime == null);
                if (taskModel.FinishedTasks.Count > 0 && unfinishedTask.Items.Count == 0)
                {
                    if (serviceOrder.ServiceOrderStatus == Domain.Enums.ServiceOrderStatus.ProcessingComplaint)
                    {
                        serviceOrder.ServiceOrderStatus = Domain.Enums.ServiceOrderStatus.DoneTaskComplaint;
                        var complaint = await _unitOfWork.ComplaintRepository.GetAsync(isTakeAll: true, expression: x => x.ServiceOrderId == serviceOrder.Id && x.ComplaintStatus == Domain.Enums.ComplaintStatus.Processing);
                        if (complaint.Items.Count != 0) {
                            complaint.Items[0].ComplaintStatus = Domain.Enums.ComplaintStatus.Completed;
                            _unitOfWork.ComplaintRepository.Update(complaint.Items[0]);
                        }     
                    }
                    else
                    {
                        serviceOrder.ServiceOrderStatus = Domain.Enums.ServiceOrderStatus.TaskFinished;
                    }
                    _unitOfWork.ServiceOrderRepository.Update(serviceOrder);
                    await _unitOfWork.SaveChangeAsync();
                }
            }
        }
        public async Task ClearProgress(Guid serviceOrderId)
        {
            var serviceOrder = await _unitOfWork.ServiceOrderRepository
                  .GetAllQueryable()
                  .AsNoTracking()
                  .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == serviceOrderId);
            if (serviceOrder == null)
            {
                throw new Exception("Không tìm thấy đơn đặt hàng dịch vụ!");
            }
            if (serviceOrder.CustomerBonsaiId != null)
            {
                var tasks = await _unitOfWork.BonsaiCareStepRepository.GetAsync(isTakeAll: true, expression: x => x.ServiceOrderId == serviceOrderId);
                foreach(BonsaiCareStep bonsaiCareStep in tasks.Items)
                {
                    bonsaiCareStep.CompletedTime = null;
                }
                _unitOfWork.BonsaiCareStepRepository.UpdateRange(tasks.Items);
                await _unitOfWork.SaveChangeAsync();
            }
            else
            {
                var tasks = await _unitOfWork.GardenCareTaskRepository.GetAsync(isTakeAll: true, expression: x => x.ServiceOrderId == serviceOrderId);
                foreach (GardenCareTask gardenCareTask in tasks.Items)
                {
                    gardenCareTask.CompletedTime = null;
                }
                _unitOfWork.GardenCareTaskRepository.UpdateRange(tasks.Items);
                await _unitOfWork.SaveChangeAsync();
            }
        }

        public async Task UpdateNote(UpdateNoteModel updateNoteModel)
        {
            var serviceOrder = await _unitOfWork.ServiceOrderRepository
                  .GetAllQueryable()
                  .AsNoTracking()
                  .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == updateNoteModel.ServiceOrderId);
            if (serviceOrder == null)
            {
                throw new Exception("Không tìm thấy đơn đặt hàng dịch vụ!");
            }
            if (serviceOrder.CustomerBonsaiId != null)
            {         
                foreach (TaskNote taskNote in updateNoteModel.TaskNotes)
                {
                    var task = await _unitOfWork.BonsaiCareStepRepository.GetAsync(isTakeAll: true, expression: x => x.ServiceOrderId == updateNoteModel.ServiceOrderId && x.Id == taskNote.TaskId);
                    if (task == null)
                    {
                        throw new Exception("Không tìm thấy công việc!");
                    }
                    task.Items[0].Note = taskNote.Note;
                    _unitOfWork.BonsaiCareStepRepository.Update(task.Items[0]);
                }
                await _unitOfWork.SaveChangeAsync();
            }
            else
            {
                foreach (TaskNote taskNote in updateNoteModel.TaskNotes)
                {
                    var task = await _unitOfWork.GardenCareTaskRepository.GetAsync(isTakeAll: true, expression: x => x.ServiceOrderId == updateNoteModel.ServiceOrderId && x.Id == taskNote.TaskId);
                    if (task == null)
                    {
                        throw new Exception("Không tìm thấy công việc!");
                    }
                    task.Items[0].Note = taskNote.Note;
                    _unitOfWork.GardenCareTaskRepository.Update(task.Items[0]);
                }
                await _unitOfWork.SaveChangeAsync();
            }
        }
    }
}
