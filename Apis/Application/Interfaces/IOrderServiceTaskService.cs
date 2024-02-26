using Application.Commons;
using Application.ViewModels.OrderServiceTaskModels;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IOrderServiceTaskService
    {
        Task<Pagination<OrderServiceTask>> GetTasks();
        Task<Pagination<OrderServiceTask>> GetDailyTasksForGarndener(Guid id);
        Task UpdateTaskProgess(OrderServiceTasksModels orderServiceTasksModels);
    }
}
