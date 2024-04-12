using Application.Commons;
using Application.Interfaces;
using Application.Repositories;
using Application.Utils;
using Application.Validations.Bonsai;
using Application.Validations.ServiceGarden;
using Application.ViewModels.BonsaiViewModel;
using Application.ViewModels.ServiceGardenViewModels;
using AutoMapper;
using Domain.Entities;
using Firebase.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace Application.Services
{
    public class ServiceGardenService : IServiceGardenService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IServiceSurchargeService _serviceSurchargeService;
        private readonly IDeliveryFeeService _deliveryFeeService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICustomerBonsaiService _customerBonsaiService;
        private readonly IdUtil _idUtil;
        public ServiceGardenService(IUnitOfWork unitOfWork, IMapper mapper, IServiceSurchargeService serviceSurchargeService, IDeliveryFeeService deliveryFeeService, UserManager<ApplicationUser> userManager, ICustomerBonsaiService customerBonsaiService, IdUtil idUtil)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _serviceSurchargeService = serviceSurchargeService;
            _deliveryFeeService = deliveryFeeService;
            _userManager = userManager;
            _customerBonsaiService = customerBonsaiService;
            _idUtil = idUtil;
        }
        public async Task<ServiceGarden> AddServiceGarden(ServiceGardenModel serviceGardenModel, Guid userId, bool isCustomer)
        {
            var garden = await _unitOfWork.CustomerGardenRepository.GetByIdAsync(serviceGardenModel.CustomerGardenId);
            if (garden == null)
            {
                throw new Exception("Không tìm thấy vườn");
            }
            var service = await _unitOfWork.ServiceRepository.GetByIdAsync(serviceGardenModel.ServiceId);
            if (service == null)
            {
                throw new Exception("Không tìm thấy dịch vụ");
            }
            var validationRules = new ServiceGardenModelValidator();
            var resultBonsaiInfo = await validationRules.ValidateAsync(serviceGardenModel);
            if (!resultBonsaiInfo.IsValid)
            {
                var errors = resultBonsaiInfo.Errors.Select(x => x.ErrorMessage);
                string errorMessage = string.Join(Environment.NewLine, errors);
                throw new Exception(errorMessage);
            }
            var serviceGarden = _mapper.Map<ServiceGarden>(serviceGardenModel);
            serviceGarden.ServiceGardenStatus = Domain.Enums.ServiceGardenStatus.UnAccepted;
            var distance = await _deliveryFeeService.GetDistanse(garden.Address);
            if (service.ServiceType == Domain.Enums.ServiceType.BonsaiCare)
            {
                if (serviceGardenModel.CustomerBonsaiId == null || serviceGardenModel.CustomerBonsaiId.Equals("00000000-0000-0000-0000-000000000000"))
                {
                    throw new Exception("Dịch vụ chăm sóc bonsai phải bao gồm bonsai");
                }
                var bonsai = await _customerBonsaiService.GetCustomerBonsaiById(serviceGardenModel.CustomerBonsaiId.Value, userId, isCustomer);
                var tasks = await _unitOfWork.CareStepRepository.GetAsync(isTakeAll: true, expression: x => x.CategoryId == bonsai.Bonsai.CategoryId && !x.IsDeleted);
                if (tasks.Items.Count == 0)
                {
                    throw new Exception("Phân loại này chưa sẵn sàng cho dịch vụ.");
                }
                var existedServiceBonsai = await _unitOfWork.ServiceGardenRepository.GetAsync(isTakeAll: true, expression: x => x.CustomerBonsaiId == serviceGardenModel.CustomerBonsaiId.Value
                &&( x.ServiceGardenStatus <= Domain.Enums.ServiceGardenStatus.StaffAccepted &&  x.ServiceGardenStatus !=Domain.Enums.ServiceGardenStatus.Cancel));
                if (existedServiceBonsai.Items.Count > 0)
                {
                    throw new Exception("Đã tồn tại đơn đăng ký thuộc về bonsai này");
                }
                    try
                    {
                        CustomerBonsai customerBonsai = new CustomerBonsai();
                        customerBonsai = await _unitOfWork.CustomerBonsaiRepository
                            .GetAllQueryable()
                            .AsNoTracking()
                            .Include(x => x.Bonsai)
                            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == serviceGardenModel.CustomerBonsaiId);
                        if (customerBonsai == null)
                        {
                            throw new Exception("Không tìm thấy bonsai!");
                        }
                        if (customerBonsai.CustomerGardenId != serviceGardenModel.CustomerGardenId)
                        {
                            throw new Exception("Bonsai không ở trong vườn!");
                        }
                        serviceGarden.TemporaryPrice = _unitOfWork.CategoryExpectedPriceRepository.GetExpectedPrice(customerBonsai.Bonsai.Height ?? 0);
                        if (customerBonsai.Bonsai.Height < 1)
                            serviceGarden.TemporaryGardener = 2;
                        else
                            if (customerBonsai.Bonsai.Height < 2)
                                serviceGarden.TemporaryGardener = 3;
                            else
                                serviceGarden.TemporaryGardener = 4;
                        serviceGarden.TemporarySurchargePrice = await _serviceSurchargeService.GetPriceByDistance(float.Parse(distance.rows[0].elements[0].distance.value.ToString())) * serviceGarden.TemporaryGardener;
                        serviceGarden.TemporaryTotalPrice = serviceGarden.TemporaryPrice + serviceGarden.TemporarySurchargePrice;
                        await _unitOfWork.ServiceGardenRepository.AddAsync(serviceGarden);
                        await _unitOfWork.SaveChangeAsync();
                        return serviceGarden;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                
            }
            var existedServiceGarden = await _unitOfWork.ServiceGardenRepository.GetAsync(isTakeAll: true, expression: x => x.CustomerGardenId == serviceGardenModel.CustomerGardenId &&
            (x.ServiceGardenStatus <= Domain.Enums.ServiceGardenStatus.StaffAccepted && x.ServiceGardenStatus != Domain.Enums.ServiceGardenStatus.Cancel));
            if (existedServiceGarden.Items.Count > 0)
            {
                throw new Exception("Đã tồn tại đơn đăng ký thuộc về bonsai này");
            }
            serviceGarden.TemporaryPrice = garden.Square * service.StandardPrice;
            TimeSpan duration = serviceGarden.EndDate - serviceGarden.StartDate;
            int daysDifference = duration.Days + 1;
            double expectedNumberGardeners = garden.Square / 50 / daysDifference;
            serviceGarden.TemporaryGardener = (int)Math.Ceiling(expectedNumberGardeners);
            serviceGarden.TemporarySurchargePrice = await _serviceSurchargeService.GetPriceByDistance(float.Parse(distance.rows[0].elements[0].distance.value.ToString())) * serviceGarden.TemporaryGardener * daysDifference;
            serviceGarden.TemporaryTotalPrice = serviceGarden.TemporaryPrice + serviceGarden.TemporarySurchargePrice;
            await _unitOfWork.ServiceGardenRepository.AddAsync(serviceGarden);
            await _unitOfWork.SaveChangeAsync();
            return serviceGarden;
        }
        public async Task CancelServiceGarden(Guid serviceGardenId)
        {
            var serviceGarden = await _unitOfWork.ServiceGardenRepository.GetByIdAsync(serviceGardenId);
            if (serviceGarden == null)
            {
                throw new Exception("Không tìm thấy đơn đặt dịch vụ");
            }
            serviceGarden.ServiceGardenStatus = Domain.Enums.ServiceGardenStatus.Cancel;
            _unitOfWork.ServiceGardenRepository.Update(serviceGarden);
            await _unitOfWork.SaveChangeAsync();
        }
        public async Task DenyServiceGarden(Guid serviceGardenId)
        {
            var serviceGarden = await _unitOfWork.ServiceGardenRepository.GetByIdAsync(serviceGardenId);
            if (serviceGarden == null)
            {
                throw new Exception("Không tìm thấy đơn đặt dịch vụ");
            }
            serviceGarden.ServiceGardenStatus = Domain.Enums.ServiceGardenStatus.Denied;
            _unitOfWork.ServiceGardenRepository.Update(serviceGarden);
            await _unitOfWork.SaveChangeAsync();
        }
        public async Task AcceptServiceGarden(Guid serviceGardenId)
        {
            var serviceGarden = await _unitOfWork.ServiceGardenRepository.GetByIdAsync(serviceGardenId);
            if (serviceGarden == null)
            {
                throw new Exception("Không tìm thấy đơn đặt dịch vụ");
            }
            serviceGarden.ServiceGardenStatus = Domain.Enums.ServiceGardenStatus.Accepted;
            _unitOfWork.ServiceGardenRepository.Update(serviceGarden);
            await _unitOfWork.SaveChangeAsync();
        }
        public async Task<Pagination<ServiceGarden>> GetServiceGarden(int pageIndex, int pageSize, bool isCustomer, Guid id)
        {
            List<Expression<Func<ServiceGarden, object>>> includes = new List<Expression<Func<ServiceGarden, object>>>{
                                 x => x.CustomerGarden,
                                 x => x.CustomerGarden.Customer.ApplicationUser,
                                 x => x.Service
                                    };
            if (isCustomer)
            {
                var customer = await _idUtil.GetCustomerAsync(id);
                var serviceGardens = await _unitOfWork.ServiceGardenRepository.GetAsync(pageIndex: pageIndex, pageSize: pageSize, expression: x => x.CustomerGarden.CustomerId == customer.Id, orderBy: query => query.OrderByDescending(x => x.CreationDate), includes: includes);
                return serviceGardens;
            }
            else
            {
              
                var serviceGardens = await _unitOfWork.ServiceGardenRepository.GetAsync(pageIndex: pageIndex, pageSize: pageSize, expression: x => x.ServiceGardenStatus != Domain.Enums.ServiceGardenStatus.UnAccepted || x.ServiceGardenStatus != Domain.Enums.ServiceGardenStatus.Cancel, orderBy: query => query.OrderByDescending(x => x.CreationDate), includes: includes);
                return serviceGardens;
            }
        }
        public async Task<ServiceGarden> GetServiceGardenById(Guid id, bool isCustomer, Guid userId)
        {
            List<Expression<Func<ServiceGarden, object>>> includes = new List<Expression<Func<ServiceGarden, object>>>{
                                 x => x.CustomerGarden,
                                 x => x.CustomerGarden.Customer.ApplicationUser,
                                 x => x.Service
                                    };

            if (isCustomer)
            {
                var customer = await _idUtil.GetCustomerAsync(userId);
                var serviceGardens = await _unitOfWork.ServiceGardenRepository.GetAsync(isTakeAll: true, expression: x => x.CustomerGarden.CustomerId == customer.Id && x.Id == id, includes: includes);
                if (serviceGardens.Items.Count == 0)
                {
                    throw new Exception("Không tìm thấy contract");
                }
                return serviceGardens.Items[0];
            }
            else
            {
                var serviceGardens = await _unitOfWork.ServiceGardenRepository.GetAsync(isTakeAll: true, expression: x =>  x.ServiceGardenStatus != Domain.Enums.ServiceGardenStatus.UnAccepted && x.Id == id,
                    isDisableTracking: true, includes: includes);
                if (serviceGardens.Items.Count == 0)
                {
                    throw new Exception("Không tìm thấy contract");
                }
                return serviceGardens.Items[0];
            }
        }
    }
}
