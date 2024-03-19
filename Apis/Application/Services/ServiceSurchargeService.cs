using Application.Commons;
using Application.Interfaces;
using Application.ViewModels.ServiceSurchargeViewModels;
using AutoMapper;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class ServiceSurchargeService : IServiceSurchargeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ServiceSurchargeService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<Pagination<ServiceSurcharge>> Get()
        {
            var serviceSurcharge = await _unitOfWork.ServiceSurchargeRepository.GetAsync(isTakeAll: true, expression: x => !x.IsDeleted);
            return serviceSurcharge;
        }
        public async Task<double?> GetPriceByDistance(float distance)
        {
            var surcharge = await _unitOfWork.ServiceSurchargeRepository.GetAllQueryable()
                .OrderBy(x => x.Distance)
                .FirstOrDefaultAsync(s => s.Distance >= distance);
            if (surcharge == null)
            {
                surcharge = await _unitOfWork.ServiceSurchargeRepository.GetAllQueryable()
                   .OrderBy(s => s.Distance)
                   .LastOrDefaultAsync();
            }

            return surcharge?.Price;
        }
        public async Task Add(ServiceSurchargeModel serviceSurchargeModel)
        {
            var serviceSurcharge = _mapper.Map<ServiceSurcharge>(serviceSurchargeModel);
            if (serviceSurcharge == null)
            {
                throw new Exception("Không được bỏ trống!");
            }
            if (serviceSurcharge.Distance == 0 || serviceSurcharge.Price == 0)
            {
                throw new Exception("Không giá trị nào được bằng 0!");
            }
            await _unitOfWork.ServiceSurchargeRepository.AddAsync(serviceSurcharge);
            await _unitOfWork.SaveChangeAsync();
        }
        public async Task Edit(Guid serviceSurchargeId,ServiceSurchargeModel serviceSurchargeModel)
        {
            var _serviceSurchange = await _unitOfWork.ServiceSurchargeRepository.GetByIdAsync(serviceSurchargeId);
            if (_serviceSurchange == null)
            {
                throw new Exception("Không tìm thấy!");
            }
            var serviceSurcharge = _mapper.Map<ServiceSurcharge>(serviceSurchargeModel);
            if (serviceSurcharge == null)
            {
                throw new Exception("Không được bỏ trống!");
            }
            if (serviceSurcharge.Distance == 0 || serviceSurcharge.Price == 0)
            {
                throw new Exception("Không giá trị nào được bằng 0!");
            }
            _unitOfWork.ServiceSurchargeRepository.Update(serviceSurcharge);
            await _unitOfWork.SaveChangeAsync();
        }
        public async Task Delete(Guid serviceSurchargeId)
        {
            var _serviceSurchange = await _unitOfWork.ServiceSurchargeRepository.GetByIdAsync(serviceSurchargeId);
            if (_serviceSurchange == null)
            {
                throw new Exception("Không tìm thấy!");
            }

            _unitOfWork.ServiceSurchargeRepository.SoftRemove(_serviceSurchange);
            await _unitOfWork.SaveChangeAsync();
        }
    }
}
