using Application.Commons;
using Application.Interfaces;
using Application.Repositories;
using Application.ViewModels.ServiceGardenViewModels;
using AutoMapper;
using Domain.Entities;
using Firebase.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Contracts;

namespace Application.Services
{
    public class ServiceGardenService : IServiceGardenService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IServiceSurchargeService _serviceSurchargeService;
        private readonly IDeliveryFeeService _deliveryFeeService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ServiceGardenService(IUnitOfWork unitOfWork, IMapper mapper, IServiceSurchargeService serviceSurchargeService, IDeliveryFeeService deliveryFeeService, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _serviceSurchargeService = serviceSurchargeService;
            _deliveryFeeService = deliveryFeeService;
            _userManager = userManager;
        }
        public async Task<ServiceGarden> AddServiceGarden(ServiceGardenModel serviceGardenModel)
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
            
            var serviceGarden = _mapper.Map<ServiceGarden>(serviceGardenModel);
            serviceGarden.ServiceGardenStatus = Domain.Enums.ServiceGardenStatus.UnAccepted;
            var distance = await _deliveryFeeService.GetDistanse(garden.Address);
            if (service.ServiceType == Domain.Enums.ServiceType.BonsaiCare)
            {
                if (serviceGardenModel.CustomerBonsaiId == null || serviceGardenModel.CustomerBonsaiId.Equals("00000000-0000-0000-0000-000000000000"))
                {
                    throw new Exception("Dịch vụ chăm sóc bonsai phải bao gồm bonsai");
                }
                else
                {
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
                        serviceGarden.TemporaryGardener = 2;
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
            }
            serviceGarden.TemporaryPrice = garden.Square * service.StandardPrice;
            TimeSpan duration = serviceGarden.EndDate - serviceGarden.StartDate;
            int daysDifference = duration.Days;
            double expectedNumberGardeners = garden.Square / 20 / daysDifference;
            serviceGarden.TemporaryGardener = (int)Math.Ceiling(expectedNumberGardeners);
            serviceGarden.TemporarySurchargePrice = await _serviceSurchargeService.GetPriceByDistance(float.Parse(distance.rows[0].elements[0].distance.value.ToString())) * serviceGarden.TemporaryGardener * daysDifference;
            serviceGarden.TemporaryTotalPrice = serviceGarden.TemporaryPrice + serviceGarden.TemporarySurchargePrice;
            await _unitOfWork.ServiceGardenRepository.AddAsync(serviceGarden);
            await _unitOfWork.SaveChangeAsync();
            return serviceGarden;
        }
        public async Task<Pagination<ServiceGarden>> GetServiceGardenByGardenId(Guid customerId, int pageIndex, int pageSize)
        {
            var customer = await GetCustomerAsync(customerId);
            var serviceGardens = await _unitOfWork.ServiceGardenRepository.GetAsync(pageIndex: pageIndex, pageSize: pageSize, expression: x => x.CustomerGarden.CustomerId == customer.Id, orderBy: query => query.OrderByDescending(x => x.CreationDate));
            return serviceGardens;
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
            serviceGarden.ServiceGardenStatus = Domain.Enums.ServiceGardenStatus.Waiting;
            _unitOfWork.ServiceGardenRepository.Update(serviceGarden);
            await _unitOfWork.SaveChangeAsync();
        }
        public async Task<Pagination<ServiceGarden>> GetServiceGarden(int pageIndex, int pageSize)
        {
            var serviceGarden = await _unitOfWork.ServiceGardenRepository.GetAsync(pageIndex: pageIndex, pageSize: pageSize, expression: x => !x.IsDeleted);
            return serviceGarden;
        }
        private async Task<Customer> GetCustomerAsync(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
                throw new Exception("Không tìm thấy!");
            var isCustomer = await _userManager.IsInRoleAsync(user, "Customer");
            if (!isCustomer)
                throw new Exception("Bạn không có quyền để thực hiện hành động này!");
            var customer = await _unitOfWork.CustomerRepository.GetAllQueryable().FirstOrDefaultAsync(x => x.UserId.ToLower().Equals(user.Id.ToLower()));
            if (customer == null)
                throw new Exception("Không tìm thấy thông tin người dùng");
            return customer;
        }
    }
}
