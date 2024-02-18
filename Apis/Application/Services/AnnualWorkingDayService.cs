using Application.Interfaces;
using Application.ViewModels.AnnualWorkingDayModel;
using Application.ViewModels.CategoryViewModels;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class AnnualWorkingDayService : IAnnualWorkingDayService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AnnualWorkingDayService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task AddWorkingday(Guid serviceOrderId,GardernerListModel gardernerListModel)
        {
            var orderService = await _unitOfWork.ServiceOrderRepository.GetByIdAsync(serviceOrderId);
            if (orderService == null)
                throw new Exception("Khong tim thay dich vu");
            List<AnnualWorkingDay> workingDays = new List<AnnualWorkingDay>();
            if (orderService.ServiceType == Domain.Enums.ServiceType.OneTime)
            {
                for (DateTime date = orderService.StartDate; date.Date <= orderService.EndDate; date = date.AddDays(1))
                {
                    foreach (Guid id in gardernerListModel.GardenerIds) 
                    {
                        workingDays.Add(new AnnualWorkingDay { ServiceOrderId = serviceOrderId, GardenerId = serviceOrderId, Date = date });
                    }            
                }
            }
            else
            {
                if (!orderService.ImplementationTime.HasValue)
                    throw new Exception("Chưa có số tháng làm việc");
                var serviceDay = await _unitOfWork.ServiceDayRepository.GetAsync(isTakeAll: true, expression: x => x.ServiceOrderId == serviceOrderId && !x.IsDeleted);
                if (serviceDay == null || serviceDay.TotalItemsCount == 0)
                    throw new Exception("Chưa có ngày làm việc");
                List<DayType> dayTypes = GetDayTypesFromServiceDays(serviceDay.Items);
                for (DateTime date = orderService.StartDate; date.Date <= orderService.StartDate.AddMonths(orderService.ImplementationTime.Value); date = date.AddDays(1))
                {
                    if (IsDesiredDayOfWeek(date, dayTypes))
                    {
                        foreach (Guid id in gardernerListModel.GardenerIds)
                        {
                            workingDays.Add(new AnnualWorkingDay { ServiceOrderId = serviceOrderId, GardenerId = id, Date = date });
                        }
                    }
                }
            }
            
            try
            {
                if (workingDays == null || workingDays.Count == 0)
                    throw new Exception("Xảy ra lỗi trong quá trình xếp lịch làm việc");
                await _unitOfWork.AnnualWorkingDayRepository.AddRangeAsync(workingDays);
                await _unitOfWork.SaveChangeAsync();
            }
            catch (Exception)
            {
                _unitOfWork.AnnualWorkingDayRepository.SoftRemoveRange(workingDays);
                throw new Exception("Đã xảy ra lỗi trong quá trình tạo mới. Vui lòng thử lại!");
            }
        }
        private List<DayType> GetDayTypesFromServiceDays(List<ServiceDay> serviceDays)
        {
            List<DayType> dayTypes = new List<DayType>();
            foreach (var serviceDay in serviceDays)
            {
                if (serviceDay.DayInWeek != null)
                {
                    dayTypes.Add(serviceDay.DayInWeek.DayType);
                }
            }
            return dayTypes;
        }
        private bool IsDesiredDayOfWeek(DateTime date, List<DayType> desiredDays)
        {
            foreach (DayType desiredDay in desiredDays)
            {
                DayOfWeek desiredDayOfWeek = (DayOfWeek)desiredDay;
                if (date.DayOfWeek == desiredDayOfWeek)
                {
                    return true;
                }
            }
            return false;
        }
        public async Task<List<AnnualWorkingDay>> GetWorkingCalencar(Guid gardenerId, int month, int year)
        {
            DateTime startDate = new DateTime(year, month, 1);
            DateTime endDate = startDate.AddMonths(1).AddDays(-1);
            var annualWorkingDays = await _unitOfWork.AnnualWorkingDayRepository.GetAsync(isTakeAll: true, expression: x => x.GardenerId == gardenerId && x.Date >= startDate && x.Date <= endDate && !x.IsDeleted);
            return annualWorkingDays.Items;
        }
    } 
}
