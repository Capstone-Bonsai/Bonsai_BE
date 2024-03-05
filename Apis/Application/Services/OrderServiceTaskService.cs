using Application.Commons;
using Application.Interfaces;
using Application.ViewModels.OrderServiceTaskModels;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using Domain.Enums;

namespace Application.Services
{
    public class OrderServiceTaskService : IOrderServiceTaskService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderServiceTaskService(IUnitOfWork unitOfWork, IMapper mapper, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
        }
        public async Task<Pagination<OrderServiceTask>> GetTasks()
        {
            var tasks = await _unitOfWork.OrderServiceTaskRepository.GetAsync(isTakeAll: true, expression: x => !x.IsDeleted, isDisableTracking: true);
            return tasks;
        }

        public async Task<Pagination<OrderServiceTask>> GetDailyTasksForGarndener(Guid id)
        {
            var gardener = await GetGardenerAsync(id.ToString().ToLower());
            if (gardener == null)
                throw new Exception("Can not found!");
            var annualWorkingDate = await _unitOfWork.AnnualWorkingDayRepository.GetAsync(isTakeAll: true, expression: x => !x.IsDeleted && x.GardenerId == gardener.Id && x.Date.Date == new DateTime(2024, 3, 20), isDisableTracking: true);
            if (annualWorkingDate == null || annualWorkingDate.Items.Count == 0)
                throw new HttpRequestException("Not found", null, System.Net.HttpStatusCode.NotFound);
            List<Expression<Func<OrderServiceTask, object>>> includes = new List<Expression<Func<OrderServiceTask, object>>>{
                                 x => x.Task
                                    };
            var tasks = await _unitOfWork.OrderServiceTaskRepository.GetAsync(isTakeAll: true, expression: x => !x.IsDeleted && x.ServiceOrderId == annualWorkingDate.Items[0].ServiceOrderId, isDisableTracking: true, includes: includes);
            return tasks;
        }
        private async Task<Gardener> GetGardenerAsync(string userId)
        {
            ApplicationUser? user = null;
            user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new Exception("Không tìm thấy người dùng!");
            var isGardener = await _userManager.IsInRoleAsync(user, "Gardener");
            if (!isGardener)
                throw new Exception("Bạn không có quyền để thực hiện hành động này!");
            var gardener = await _unitOfWork.GardenerRepository.GetAllQueryable().FirstOrDefaultAsync(x => x.UserId.ToLower().Equals(user.Id.ToLower()));
            if (gardener == null)
                throw new Exception("Không tìm thấy thông tin người dùng");
            return gardener;
        }
        public async Task UpdateTaskProgess(OrderServiceTasksModels orderServiceTasksModels)
        {
            if (orderServiceTasksModels.OrderServiceTaskId.Count == 0 || orderServiceTasksModels.IsFinished.Count == 0)
                throw new Exception("Không có công việc!");
            for (int i = 0; i < orderServiceTasksModels.OrderServiceTaskId.Count; i++)
            {
                var orderServiceTask = await _unitOfWork.OrderServiceTaskRepository.GetByIdAsync(orderServiceTasksModels.OrderServiceTaskId[i]);
                if (orderServiceTask == null)
                {
                    throw new Exception("Không tìm thấy công việc!");
                }
                if (orderServiceTasksModels.IsFinished[i] == true)
                    orderServiceTask.ServiceTaskStatus = ServiceTaskStatus.Finished;
                else
                    orderServiceTask.ServiceTaskStatus = ServiceTaskStatus.Working;
                orderServiceTask.CompletedDate = DateTime.Now;
                _unitOfWork.OrderServiceTaskRepository.Update(orderServiceTask);
                await _unitOfWork.SaveChangeAsync();
            }

        }
    }
}
