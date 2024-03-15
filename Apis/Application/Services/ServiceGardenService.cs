﻿using Application.Commons;
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
                        throw new Exception("Không tìm thấy bonsai");
                    }
                    await _unitOfWork.ServiceGardenRepository.AddAsync(serviceGarden);
                    await _unitOfWork.SaveChangeAsync();
                    //implement add time
                    return serviceGarden;
                }
            }
            serviceGarden.TemporaryPrice = garden.Square / service.StandardPrice;
            serviceGarden.TemporarySurchargePrice = 0;
            serviceGarden.TemporaryTotalPrice = serviceGarden.TemporaryPrice + serviceGarden.TemporarySurchargePrice;
            serviceGarden.CustomerGardenStatus = Domain.Enums.CustomerGardenStatus.Waiting;
            await _unitOfWork.ServiceGardenRepository.AddAsync(serviceGarden);
            await _unitOfWork.SaveChangeAsync();
            return serviceGarden;
        }
        public async Task<Pagination<ServiceGarden>> GetServiceGardenByGardenId(Guid customerGardenId, int pageIndex, int pageSize)
        {
            var serviceGardens = await _unitOfWork.ServiceGardenRepository.GetAsync(pageIndex: pageIndex, pageSize: pageSize, expression: x => x.CustomerGardenId == customerGardenId, orderBy: query => query.OrderBy(x => x.CustomerGardenStatus));
            return serviceGardens;
        }
        public async Task CancelServiceGarden(Guid serviceGardenId)
        {
            var serviceGarden = await _unitOfWork.ServiceGardenRepository.GetByIdAsync(serviceGardenId);
            if (serviceGarden == null)
            {
                throw new Exception("Không tìm thấy đơn đặt dịch vụ");
            }
            serviceGarden.CustomerGardenStatus = Domain.Enums.CustomerGardenStatus.Cancel;
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
            serviceGarden.CustomerGardenStatus = Domain.Enums.CustomerGardenStatus.Denied;
            _unitOfWork.ServiceGardenRepository.Update(serviceGarden);
            await _unitOfWork.SaveChangeAsync();
        }
    }
}
