using Application.ViewModels.AnnualWorkingDayModel;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IAnnualWorkingDayService
    {
        Task AddWorkingday(Guid serviceOrderId, GardernerListModel gardernerListModel);
        Task<List<AnnualWorkingDay>> GetWorkingCalencar(Guid gardenerId, int month, int year);
    }
}
