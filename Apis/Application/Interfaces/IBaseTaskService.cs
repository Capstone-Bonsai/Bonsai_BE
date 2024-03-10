using Application.Commons;
using Application.ViewModels.BaseTaskViewTasks;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IBaseTaskService
    {
        public Task<IList<BaseTask>> GetBaseTasks();
        public Task<Pagination<BaseTask>> GetBaseTasksPagination(int page = 0, int size = 10);
        public Task<BaseTask> GetBaseTaskById(Guid id);
        public Task<List<string>> AddBaseTask(BaseTaskModel model);
        public Task<List<string>> UpdateBaseTask(Guid id, BaseTaskModel model);
        public Task DeleteBaseTask(Guid id);
    }
}
