using Application.Commons;
using Application.Interfaces;
using Application.Validations.ServiceOrder;
using Application.Validations.Tag;
using Application.ViewModels.ServiceOrderViewModels;
using Application.ViewModels.TagViewModels;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Firebase.Auth;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class ServiceOrderService : IServiceOrderService
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
        public async Task<Pagination<ServiceOrder>> GetServiceOrders()
        {
            var serviceOrders = await _unitOfWork.ServiceOrderRepository.GetAsync(isTakeAll: true, expression: x => !x.IsDeleted, isDisableTracking: true);
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
                          (100 - (service.DiscountPercent ?? 0)) / 100,
                ServiceStatus = ServiceStatus.Waiting
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
        public async Task ResponseServiceOrder(Guid id, ResponseServiceOrderModel responseServiceOrderModel)
        {
            if (responseServiceOrderModel == null)
                throw new ArgumentNullException(nameof(responseServiceOrderModel), "Vui lòng nhập thêm thông tin phân loại!");
            var validationRules = new ResponseServiceOrderModelValidator();
            var resultServiceOrderInfo = await validationRules.ValidateAsync(responseServiceOrderModel);
            if (!resultServiceOrderInfo.IsValid)
            {
                var errors = resultServiceOrderInfo.Errors.Select(x => x.ErrorMessage);
                string errorMessage = string.Join(Environment.NewLine, errors);
                throw new Exception(errorMessage);
            }
            var serviceOrder = await _unitOfWork.ServiceOrderRepository.GetByIdAsync(id);
            if (serviceOrder == null)
                throw new Exception("Không tìm thấy đơn đặt dịch vụ");
            Order? order = null;
            if (responseServiceOrderModel.OrderId != null)
            {
                order = await _unitOfWork.OrderRepository.GetByIdAsync(responseServiceOrderModel.OrderId.Value);
            }
            try
            {
                if (order != null)
                {
                    serviceOrder.OrderId = order.Id;
                    serviceOrder.OrderPrice = order.Price;
                }
                serviceOrder.ResponseGardenSquare = responseServiceOrderModel.ResponseGardenSquare;
                serviceOrder.ResponseStandardSquare = responseServiceOrderModel.ResponseStandardSquare;
                serviceOrder.ResponsePrice = responseServiceOrderModel.ResponsePrice;
                serviceOrder.ResponseWorkingUnit = responseServiceOrderModel.ResponseWorkingUnit;
                serviceOrder.ResponseTotalPrice = responseServiceOrderModel.ResponseTotalPrice;
                serviceOrder.ResponseFinalPrice = responseServiceOrderModel.ResponseFinalPrice;
                serviceOrder.NumberGardener = responseServiceOrderModel.NumberGardener;
                serviceOrder.ServiceStatus = ServiceStatus.Applied;
                _unitOfWork.ServiceOrderRepository.Update(serviceOrder);
                await _unitOfWork.SaveChangeAsync();
            }
            catch (Exception)
            {
                throw new Exception("Đã xảy ra lỗi trong quá trình cập nhật. Vui lòng thử lại!");
            }
        }
    }
}
