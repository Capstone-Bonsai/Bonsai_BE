using Application.Commons;
using Application.Interfaces;
using Application.ViewModels.CategoryViewModels;
using Application.ViewModels.ServiceModels;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class ServiceService : IServiceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ServiceService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<Pagination<Service>> GetServices()
        {
            var services = await _unitOfWork.ServiceRepository.GetAsync(isTakeAll: true, expression: x => !x.IsDeleted, isDisableTracking: true);
            return services;
        }
        public async Task AddService(ServiceModel serviceModel)
        {
            var checkService = await _unitOfWork.ServiceRepository.GetAsync(isTakeAll: true, expression: x => x.Name.ToLower().Equals(serviceModel.Name.ToLower()) && !x.IsDeleted, isDisableTracking: true);
            if (checkService.Items.Count > 0)
                throw new Exception("Dịch vụ này đã tồn tại!");
            var service = _mapper.Map<Service>(serviceModel);
            try
            {
                _unitOfWork.BeginTransaction();
                await _unitOfWork.ServiceRepository.AddAsync(service);
                foreach (Guid id in serviceModel.TaskId)
                {
                    if (await _unitOfWork.TasksRepository.GetByIdAsync(id) == null)
                    {
                        throw new Exception();
                    }
                    await _unitOfWork.BaseTaskRepository.AddAsync(new BaseTask()
                    {
                        ServiceId = service.Id,
                        TaskId = id
                    });
                }
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception)
            {
                _unitOfWork.RollbackTransaction();
                throw new Exception("Đã xảy ra lỗi trong quá trình tạo mới. Vui lòng thử lại!");
            }
        }
        public async Task<Service?> GetServiceById(Guid id)
        {
            var service = await _unitOfWork.ServiceRepository.GetByIdAsync(id);
            return service;
        }
        public async Task UpdateService(Guid id, ServiceModel serviceModel)
        {
            var checkService = await _unitOfWork.ServiceRepository.GetAsync(isTakeAll: true, expression: x => x.Name.ToLower().Equals(serviceModel.Name.ToLower()) && !x.IsDeleted, isDisableTracking: true);
            if (checkService.Items.Count > 0)
                throw new Exception("Dịch vụ  này đã tồn tại!");
            var service = _mapper.Map<Service>(serviceModel);
            service.Id = id;
            var result = await _unitOfWork.ServiceRepository.GetByIdAsync(service.Id);
            if (result == null)
                throw new Exception("Không tìm thấy dịch vụ!");
            try
            {
                _unitOfWork.ServiceRepository.Update(service);
                await _unitOfWork.SaveChangeAsync();
            }
            catch (Exception)
            {
                throw new Exception("Đã xảy ra lỗi trong quá trình cập nhật. Vui lòng thử lại!");
            }
        }
        public async Task DeleteService(Guid id)
        {
            var result = await _unitOfWork.ServiceRepository.GetByIdAsync(id);
            if (result == null)
                throw new Exception("Không tìm thấy dịch vụ!");
            try
            {
                _unitOfWork.ServiceRepository.SoftRemove(result);
                await _unitOfWork.SaveChangeAsync();
            }
            catch (Exception)
            {
                throw new Exception("Đã xảy ra lỗi trong quá trình xóa dịch vụ. Vui lòng thử lại!");
            }
        }
    }
}
