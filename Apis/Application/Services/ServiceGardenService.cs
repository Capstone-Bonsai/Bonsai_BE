using Application.Interfaces;
using Application.ViewModels.ServiceGardenViewModels;
using AutoMapper;
using Domain.Entities;
using Firebase.Auth;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class ServiceGardenService : IServiceGardenService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ServiceGardenService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task AddServiceGarden(ServiceGardenModel serviceGardenModel)
        {
            var garden = await _unitOfWork.CustomerGardenRepository.GetByIdAsync(serviceGardenModel.CustomerGardenId);
            if (garden == null)
            {
                throw new Exception("Không tìm thấy vườn");
            }
            var service = await _unitOfWork.ServiceRepository.GetByIdAsync(serviceGardenModel.ServiceId);
            if (service == null)
            {
                throw new Exception("Không tìm thấy vườn");
            }
            
            var serviceGarden = _mapper.Map<ServiceGarden>(serviceGardenModel);
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
                        .ThenInclude(x => x.Category).FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == serviceGardenModel.CustomerBonsaiId);
                    if (customerBonsai == null)
                    {
                        throw new Exception("Không tìm thấy bonsai");
                    }

                }
            }
            serviceGarden.TemporaryPrice = garden.Square / service.StandardPrice;
            // implement surcharge price calculator
            serviceGarden.TemporarySurchargePrice = 0;
            serviceGarden.TemporaryTotalPrice = serviceGarden.TemporaryPrice + serviceGarden.TemporarySurchargePrice;
            serviceGarden.CustomerGardenStatus = Domain.Enums.CustomerGardenStatus.Waiting;
            await _unitOfWork.ServiceGardenRepository.AddAsync(serviceGarden);
            await _unitOfWork.SaveChangeAsync();
        }
    }
}
