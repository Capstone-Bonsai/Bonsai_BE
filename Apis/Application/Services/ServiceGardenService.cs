using Application.Commons;
using Application.Interfaces;
using Application.Repositories;
using Application.ViewModels.ServiceGardenViewModels;
using AutoMapper;
using Domain.Entities;
using Firebase.Auth;
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

        public ServiceGardenService(IUnitOfWork unitOfWork, IMapper mapper, IServiceSurchargeService serviceSurchargeService, IDeliveryFeeService deliveryFeeService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _serviceSurchargeService = serviceSurchargeService;
            _deliveryFeeService = deliveryFeeService;
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
            if (service.ServiceType == Domain.Enums.ServiceType.BonsaiCare)
            {
                if (serviceGardenModel.CustomerBonsaiId == null || serviceGardenModel.CustomerBonsaiId.Equals("00000000-0000-0000-0000-000000000000"))
                {
                    throw new Exception("Dịch vụ chăm sóc bonsai phải bao gồm bonsai");
                }
                else
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
                    await _unitOfWork.ServiceGardenRepository.AddAsync(serviceGarden);
                    await _unitOfWork.SaveChangeAsync();
                    //implement add time
                    serviceGarden.TemporaryPrice = _unitOfWork.CategoryExpectedPriceRepository.GetExpectedPrice(customerBonsai.Bonsai.Height ?? 0);
                    return serviceGarden;
                }
            }
            var distance = await _deliveryFeeService.GetDistanse(garden.Address);
            serviceGarden.TemporaryPrice = garden.Square * service.StandardPrice;
            serviceGarden.TemporarySurchargePrice = await _serviceSurchargeService.GetPriceByDistance(float.Parse(distance.rows[0].elements[0].distance.value.ToString()));
            serviceGarden.TemporaryTotalPrice = serviceGarden.TemporaryPrice + serviceGarden.TemporarySurchargePrice;
            serviceGarden.ServiceGardenStatus = Domain.Enums.ServiceGardenStatus.Waiting;
            await _unitOfWork.ServiceGardenRepository.AddAsync(serviceGarden);
            await _unitOfWork.SaveChangeAsync();
            return serviceGarden;
        }
        public async Task<Pagination<ServiceGarden>> GetServiceGardenByGardenId(Guid customerGardenId, int pageIndex, int pageSize)
        {
            var serviceGardens = await _unitOfWork.ServiceGardenRepository.GetAsync(pageIndex: pageIndex, pageSize: pageSize, expression: x => x.CustomerGardenId == customerGardenId, orderBy: query => query.OrderBy(x => x.ServiceGardenStatus));
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
        public async Task<Pagination<ServiceGarden>> GetServiceGarden(int pageIndex, int pageSize)
        {
            var serviceGarden = await _unitOfWork.ServiceGardenRepository.GetAsync(pageIndex: pageIndex, pageSize: pageSize, expression: x => !x.IsDeleted);
            return serviceGarden;
        }
    }
}
