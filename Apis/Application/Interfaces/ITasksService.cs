using Application.Commons;
using Application.ViewModels.TagViewModels;
using Application.ViewModels.TasksViewModels;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ITasksService
    {
        Task<Pagination<Tasks>> GetTasks();
        Task<Tasks?> GetTaskById(Guid id);
        Task AddTask(TasksModel tasksModel);
        Task UpdateTask(Guid id, TasksModel tasksModel);
        Task DeleteTask(Guid id);
    }
}
