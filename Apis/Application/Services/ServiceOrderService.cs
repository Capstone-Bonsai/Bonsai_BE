using Application.Commons;
using Application.Validations.ServiceOrder;
using Application.Validations.Tag;
using Application.ViewModels.ServiceOrderViewModels;
using Application.ViewModels.TagViewModels;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Firebase.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class ServiceOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ServiceOrderService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<Pagination<ServiceOrder>> GetServiceOrdersPagination(int pageIndex, int pageSize)
        {
            var serviceOrders = await _unitOfWork.ServiceOrderRepository.GetAsync(pageIndex: pageIndex, pageSize: pageSize, expression: x => !x.IsDeleted, isDisableTracking: true);
            return serviceOrders;
        }
        public async Task<Pagination<ServiceOrder>> GetServiceOrders(int pageIndex, int pageSize)
        {
            var serviceOrders = await _unitOfWork.ServiceOrderRepository.GetAsync(pageIndex: pageIndex, pageSize: pageSize, expression: x => !x.IsDeleted, isDisableTracking: true);
            return serviceOrders;
        }
        public async Task AddServiceOrder(ServiceOrderModel serviceOrderModel, Guid customerId)
        {
            if (serviceOrderModel == null)
                throw new ArgumentNullException(nameof(serviceOrderModel), "Vui lòng nhập thêm thông tin phân loại!");
            var validationRules = new ServiceOrderModelValidator();
            var resultServiceOrderInfo = await validationRules.ValidateAsync(serviceOrderModel);
            if (!resultServiceOrderInfo.IsValid)
            {
                var errors = resultServiceOrderInfo.Errors.Select(x => x.ErrorMessage);
                string errorMessage = string.Join(Environment.NewLine, errors);
                throw new Exception(errorMessage);
            }
            var service = await _unitOfWork.ServiceRepository.GetByIdAsync(serviceOrderModel.ServiceId);
            if (service == null)
                throw new Exception("Không tìm thấy dịch vụ yêu cầu!");

            var serviceOrder = new ServiceOrder() {
                CustomerId = customerId,
                ServiceId = serviceOrderModel.ServiceId,
                ServiceType = service.ServiceType,
                Address = serviceOrderModel.Address,
                StandardSquare = service.StandardSqure,
                StandardPrice = service.StandardPrice,
                DiscountPercent = service.DiscountPercent,
                ImplementationTime = service.ImplementationTime,
                StartDate = serviceOrderModel.StartDate,
                EndDate = serviceOrderModel.EndDate,
                GardenSquare = serviceOrderModel.GardenSquare,
                ExpectedWorkingUnit = serviceOrderModel.GardenSquare / service.StandardSqure,
                TemporaryPrice = (serviceOrderModel.GardenSquare / service.StandardSqure) * service.StandardPrice,
                TemporaryTotalPrice = ((serviceOrderModel.GardenSquare / service.StandardSqure) * service.StandardPrice) *
                          (100 - (service.DiscountPercent ?? 0)) / 100
            };
            try
            {
                await _unitOfWork.ServiceOrderRepository.AddAsync(serviceOrder);
                await _unitOfWork.SaveChangeAsync();
            }
            catch (Exception)
            {
                _unitOfWork.ServiceOrderRepository.SoftRemove(serviceOrder);
                throw new Exception("Đã xảy ra lỗi trong quá trình tạo mới. Vui lòng thử lại!");
            }
        }
    }
}
