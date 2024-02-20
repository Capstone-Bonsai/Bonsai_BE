using Application.Commons;
using Application.Interfaces;
using Application.Validations.ServiceOrder;
using Application.Validations.Tag;
using Application.ViewModels.OrderViewModels;
using Application.ViewModels.ProductViewModels;
using Application.ViewModels.ServiceOrderViewModels;
using Application.ViewModels.TagViewModels;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Firebase.Auth;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class ServiceOrderService : IServiceOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly FirebaseService _fireBaseService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ServiceOrderService(IUnitOfWork unitOfWork, IMapper mapper, FirebaseService fireBaseService, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _fireBaseService = fireBaseService;
            _userManager = userManager;
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
            bool operationSuccessful = false;

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
            var customer = await GetCustomerAsync(customerId.ToString().ToLower());
            var serviceOrder = new ServiceOrder()
            {
                Id = new Guid(),
                CustomerId = customer.Id,
                CreationDate = DateTime.Now,
                ServiceId = service.Id,
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
                _unitOfWork.BeginTransaction();
                await _unitOfWork.ServiceOrderRepository.AddAsync(serviceOrder);
                if (serviceOrderModel.Image != null)
                {
                    foreach (var singleImage in serviceOrderModel.Image.Select((image, index) => (image, index)))
                    {
                        string newImageName = serviceOrder.Id + "_i" + singleImage.index;
                        string folderName = $"serviceOrder/{serviceOrder.Id}/Image";
                        string imageExtension = Path.GetExtension(singleImage.image.FileName);
                        //Kiểm tra xem có phải là file ảnh không.
                        string[] validImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
                        const long maxFileSize = 20 * 1024 * 1024;
                        if (Array.IndexOf(validImageExtensions, imageExtension.ToLower()) == -1 || singleImage.image.Length > maxFileSize)
                        {
                            throw new Exception("Có chứa file không phải ảnh hoặc quá dung lượng tối đa(>20MB)!");
                        }
                        var url = await _fireBaseService.UploadFileToFirebaseStorage(singleImage.image, newImageName, folderName);
                        if (url == null)
                            throw new Exception("Lỗi khi đăng ảnh lên firebase!");

                        ServiceImage serviceImage = new ServiceImage()
                        {
                            ServiceOrderId = serviceOrder.Id,
                            ImageUrl = url
                        };

                        await _unitOfWork.ServiceImageRepository.AddAsync(serviceImage);
                    }
                }
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception)
            {
                _unitOfWork.RollbackTransaction();
                if (operationSuccessful)
                {
                    foreach (var singleImage in serviceOrderModel.Image.Select((image, index) => (image, index)))
                    {
                        string newImageName = serviceOrder.Id + "_i" + singleImage.index;
                        string folderName = $"serviceOrder/{serviceOrder.Id}/Image";
                        await _fireBaseService.DeleteFileInFirebaseStorage(newImageName, folderName);
                    }
                }
                throw;
            }
        }
        private async Task<Customer> GetCustomerAsync(string userId)
        {
            ApplicationUser? user = null;
            /*if (userId == null || userId.Equals("00000000-0000-0000-0000-000000000000"){ }*/
                user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new Exception("Đã xảy ra lỗi trong quá trình đặt hàng!");
            var isCustomer = await _userManager.IsInRoleAsync(user, "Customer");
            if (!isCustomer)
                throw new Exception("Bạn không có quyền để thực hiện hành động này!");
            var customer = await _unitOfWork.CustomerRepository.GetAllQueryable().FirstOrDefaultAsync(x => x.UserId.ToLower().Equals(user.Id.ToLower()));
            if (customer == null)
                throw new Exception("Không tìm thấy thông tin người dùng");
            return customer;
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
        public async Task<ServiceOrder?> GetOrderServiceById(Guid id)
        {
            Pagination<ServiceOrder> orderService;
            List<Expression<Func<ServiceOrder, object>>> includes = new List<Expression<Func<ServiceOrder, object>>>{
                                 x => x.Service,
                                 x => x.Customer.ApplicationUser,
                                 x => x.ServiceImages
                                    };
            orderService = await _unitOfWork.ServiceOrderRepository.GetAsync(isTakeAll: true, expression: x => !x.IsDeleted & x.Id == id,
                isDisableTracking: true, includes: includes);
            return orderService.Items[0];
        }
    }
}
