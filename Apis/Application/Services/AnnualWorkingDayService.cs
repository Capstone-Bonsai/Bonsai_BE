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
    public class AnnualWorkingDayService
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
                if (orderService.DateType == null || !orderService.DateType.HasValue)
                    throw new Exception("Chưa có số tháng làm việc");
                for (DateTime date = orderService.StartDate; date.Date <= orderService.StartDate.AddMonths(orderService.ImplementationTime.Value); date = date.AddDays(1))
                {
                    if (IsDesiredDayOfWeek(date, orderService.DateType.Value))
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
        private bool IsDesiredDayOfWeek(DateTime date, DayType desiredDay)
        {
            DayOfWeek desiredDayOfWeek = (DayOfWeek)desiredDay;
            return date.DayOfWeek == desiredDayOfWeek;
        }
    } 
}
