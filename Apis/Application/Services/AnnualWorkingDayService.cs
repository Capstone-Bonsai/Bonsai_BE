using Application.Commons;
using Application.Interfaces;
using Application.ViewModels.AnnualWorkingDayModel;
using Application.ViewModels.CategoryViewModels;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class AnnualWorkingDayService : IAnnualWorkingDayService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;

        public AnnualWorkingDayService(IUnitOfWork unitOfWork, IMapper mapper, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
        }
        public async Task AddWorkingday(Guid serviceOrderId,GardernerListModel gardernerListModel)
        {
            var orderService = await _unitOfWork.ServiceOrderRepository.GetByIdAsync(serviceOrderId);
            if (orderService == null)
                throw new Exception("Khong tim thay dich vu");
            List<AnnualWorkingDay> workingDays = new List<AnnualWorkingDay>();
            if (orderService.ServiceType == Domain.Enums.ServiceType.OneTime)
            {
                if(orderService.NumberGardener != gardernerListModel.GardenerIds.Count)
                for (DateTime date = orderService.StartDate; date.Date <= orderService.EndDate; date = date.AddDays(1))
                {
                    foreach (Guid id in gardernerListModel.GardenerIds) 
                    {
                        var gardener = await GetGardenerAsync(id.ToString().ToLower());
                        workingDays.Add(new AnnualWorkingDay { ServiceOrderId = serviceOrderId, GardenerId = gardener.Id, Date = date });
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
                            var gardener = await GetGardenerAsync(id.ToString().ToLower());
                            workingDays.Add(new AnnualWorkingDay { ServiceOrderId = serviceOrderId, GardenerId = gardener.Id, Date = date });
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
        private async Task<Gardener> GetGardenerAsync(string userId)
        {
            ApplicationUser? user = null;
            /*if (userId == null || userId.Equals("00000000-0000-0000-0000-000000000000"){ }*/
            user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new Exception("Đã xảy ra lỗi trong quá trình đặt hàng!");
            var isGardener = await _userManager.IsInRoleAsync(user, "Gardener");
            if (!isGardener)
                throw new Exception("Đây không phải tài khoản của người làm vườn!");
            var gardener = await _unitOfWork.GardenerRepository.GetAllQueryable().FirstOrDefaultAsync(x => x.UserId.ToLower().Equals(user.Id.ToLower()));
            if (gardener == null)
                throw new Exception("Không tìm thấy thông tin người dùng");
            return gardener;
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
            var gardener = await GetGardenerAsync(gardenerId.ToString().ToLower());
            List<Expression<Func<AnnualWorkingDay, object>>> includes = new List<Expression<Func<AnnualWorkingDay, object>>>{
                                 x => x.ServiceOrder
                                    };
            var annualWorkingDays = await _unitOfWork.AnnualWorkingDayRepository.GetAsync(isTakeAll: true, expression: x => x.GardenerId == gardener.Id && x.Date >= startDate && x.Date <= endDate && !x.IsDeleted, includes: includes);
            return annualWorkingDays.Items;
        }
        public async Task<Pagination<AnnualWorkingDay>> GetAnnualWorkingDays()
        {
            var annualWorkingDay = await _unitOfWork.AnnualWorkingDayRepository.GetAsync(isTakeAll: true, expression: x => !x.IsDeleted,
                isDisableTracking: true);
            return annualWorkingDay;
        }
    } 
}
