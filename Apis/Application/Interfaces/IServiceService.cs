using Application.Commons;
using Application.ViewModels.ServiceModels;
using Application.ViewModels.SubCategoryViewModels;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IServiceService
    {
        Task<Pagination<Service>> GetServices();
        Task<Service?> GetServiceById(Guid id);
        Task AddService(ServiceModel serviceModel);
        Task UpdateService(Guid id, ServiceModel serviceModel);
        Task DeleteService(Guid id);
    }
}
