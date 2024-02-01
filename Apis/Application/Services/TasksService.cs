using Application.Commons;
using Application.Interfaces;
using Application.Validations.Tag;
using Application.ViewModels.TagViewModels;
using Application.ViewModels.TasksViewModels;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class TasksService : ITasksService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public TasksService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<Pagination<Tasks>> GetTasks()
        {
            var tasks = await _unitOfWork.TasksRepository.GetAsync(isTakeAll: true, expression: x => !x.IsDeleted, isDisableTracking: true);
            return tasks;
        }
        public async Task<Tasks?> GetTaskById(Guid id)
        {
            var tasks = await _unitOfWork.TasksRepository.GetByIdAsync(id);
            return tasks;
        }
        public async Task AddTask(TasksModel tasksModel)
        {
            if (tasksModel == null)
                throw new ArgumentNullException(nameof(tasksModel), "Vui lòng nhập tên công việc!");
            var checkTasks = await _unitOfWork.TagRepository.GetAsync(isTakeAll: true, expression: x => x.Name.ToLower().Equals(tasksModel.Name.ToLower()) && !x.IsDeleted, isDisableTracking: true);
            if (checkTasks.Items.Count > 0)
                throw new Exception("Công việc này đã tồn tại!");
            var tasks = _mapper.Map<Tasks>(tasksModel);
            try
            {
                await _unitOfWork.TasksRepository.AddAsync(tasks);
                await _unitOfWork.SaveChangeAsync();
            }
            catch (Exception)
            {
                _unitOfWork.TasksRepository.SoftRemove(tasks);
                throw new Exception("Đã xảy ra lỗi trong quá trình tạo mới. Vui lòng thử lại!");
            }
        }
        public async Task UpdateTask(Guid id, TasksModel tasksModel)
        {
            var checkTasks = await _unitOfWork.TagRepository.GetAsync(isTakeAll: true, expression: x => x.Name.ToLower().Equals(tasksModel.Name.ToLower()) && !x.IsDeleted, isDisableTracking: true);
            if (checkTasks.Items.Count > 0)
                throw new Exception("Phân loại này đã tồn tại!");
            var tasks = _mapper.Map<Tasks>(tasksModel);
            tasks.Id = id;
            var result = await _unitOfWork.TasksRepository.GetByIdAsync(tasks.Id);
            if (result == null)
                throw new Exception("Không tìm thấy sản phẩm!");
            try
            {
                _unitOfWork.TasksRepository.Update(tasks);
                await _unitOfWork.SaveChangeAsync();
            }
            catch (Exception)
            {
                throw new Exception("Đã xảy ra lỗi trong quá trình cập nhật. Vui lòng thử lại!");
            }
        }
        public async Task DeleteTask(Guid id)
        {
            var result = await _unitOfWork.TasksRepository.GetByIdAsync(id);
            if (result == null)
                throw new Exception("Không tìm thấy phân loại!");
            try
            {
                _unitOfWork.TasksRepository.SoftRemove(result);
                await _unitOfWork.SaveChangeAsync();
            }
            catch (Exception)
            {
                throw new Exception("Đã xảy ra lỗi trong quá trình xóa sản phẩm. Vui lòng thử lại!");
            }
        }
    }
}
