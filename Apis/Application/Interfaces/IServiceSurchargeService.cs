using Application.Commons;
using Application.ViewModels.ServiceSurchargeViewModels;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IServiceSurchargeService
    {
        Task<Pagination<ServiceSurcharge>> Get();
        Task<double?> GetPriceByDistance(float distance);
        Task Add(ServiceSurchargeModel serviceSurchargeModel);
        Task Edit(Guid serviceSurchargeId, ServiceSurchargeModel serviceSurchargeModel);
        Task Delete(Guid serviceSurchargeId);
    }
}
