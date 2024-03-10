using Application.Commons;
using Application.Interfaces;
using Application.Validations.BaseTasks;
using Application.Validations.Order;
using Application.ViewModels.BaseTaskViewTasks;
using Application.ViewModels.CategoryViewModels;
using AutoMapper;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class BaseTaskService : IBaseTaskService
    {
        private readonly IUnitOfWork _unit;
        private readonly IMapper _mapper;

        public BaseTaskService(IUnitOfWork unit, IMapper mapper)
        {
            _unit = unit;
            _mapper = mapper;
        }

        public async Task<List<string>> AddBaseTask(BaseTaskModel model)
        {
            if (model == null)
            {
                throw new Exception("Vui lòng thêm các thông tin mua hàng.");
            }
            var valiadte = new BaseTaskModelValidator();
            var results = await valiadte.ValidateAsync(model);
            if (!results.IsValid)
            {
                var errors = new List<string>();
                errors.AddRange(results.Errors.Select(x => x.ErrorMessage));
                return errors;
            }
            var list = await _unit.BaseTaskRepository.GetAsync(isTakeAll: true, expression: x => x.Name.ToLower().Equals(model.Name.ToLower()) && !x.IsDeleted, isDisableTracking: true);
            if (list.Items.Count > 0)
                throw new Exception("Tên nhiệm vụ này đã tồn tại!");
            var task = _mapper.Map<BaseTask>(model);
            try
            {
                await _unit.BaseTaskRepository.AddAsync(task);
                await _unit.SaveChangeAsync();
                return null;
            }
            catch (Exception)
            {
                throw new Exception("Đã xảy ra lỗi trong quá trình tạo mới. Vui lòng thử lại!");
            }
        }

        public async Task DeleteBaseTask(Guid id)
        {

            var task = await _unit.BaseTaskRepository.GetAllQueryable().Include(x => x.GardenCareTasks).Include(x => x.ServiceBaseTasks).FirstOrDefaultAsync(x => x.Id == id);
            if (task == null)
            {
                throw new Exception("Không tìm thấy");
            }
            else if ((task.GardenCareTasks == null|| task.GardenCareTasks.Count ==0) && (task.ServiceBaseTasks == null||task.ServiceBaseTasks.Count ==0))
            {
                _unit.BaseTaskRepository.SoftRemove(task);
                await _unit.SaveChangeAsync();
            }
            else
            {
                throw new Exception("Không thể xóa vì vẫn còn nhiệm vụ trong dịch vụ và vườn.");
            }


        }

        public async Task<BaseTask> GetBaseTaskById(Guid id)
        {
            var task = await _unit.BaseTaskRepository.GetByIdAsync(id);
            if (task == null)
                throw new Exception("Không tìm thấy");
            else
                return task;
        }

        public async Task<IList<BaseTask>> GetBaseTasks()
        {
            var tasks = await _unit.BaseTaskRepository.GetAllQueryable().AsNoTracking().Where(x => !x.IsDeleted).OrderByDescending(x => x.CreationDate).ToListAsync();
            return tasks;
        }

        public async Task<Pagination<BaseTask>> GetBaseTasksPagination(int page = 0, int size = 10)
        {
            Pagination<BaseTask> tasks;
            tasks = await _unit.BaseTaskRepository.GetAsync(pageIndex: page, pageSize: size, expression: x => !x.IsDeleted);
            return tasks;
        }

        public async Task<List<string>> UpdateBaseTask(Guid id, BaseTaskModel model)
        {


            if (model == null)
            {
                throw new Exception("Vui lòng thêm các thông tin mua hàng.");
            }
            var valiadte = new BaseTaskModelValidator();
            var results = await valiadte.ValidateAsync(model);
            if (!results.IsValid)
            {
                var errors = new List<string>();
                errors.AddRange(results.Errors.Select(x => x.ErrorMessage));
                return errors;
            }
            var task = await _unit.BaseTaskRepository.GetAllQueryable().AsNoTracking().FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == id);
            if (task == null)
                throw new Exception("Không tìm thầy");
            var list = await _unit.BaseTaskRepository.GetAsync(isTakeAll: true, expression: x => x.Name.ToLower().Equals(model.Name.ToLower()) && x.Id != id && !x.IsDeleted, isDisableTracking: true);
            if (list.Items.Count > 0)
                throw new Exception("Tên nhiệm vụ này đã tồn tại!");
            
            task.Name = model.Name;
            task.Detail = model.Detail;
            _unit.BaseTaskRepository.Update(task);
            await _unit.SaveChangeAsync();
            return null;
        }



    }
}
