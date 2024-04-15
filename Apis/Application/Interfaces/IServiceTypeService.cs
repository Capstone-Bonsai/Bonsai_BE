using Application.Commons;
using Application.ViewModels.ServiceTypeViewModels;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IServiceTypeService
    {
        Task<Pagination<ServiceType>> Get();
        Task Update(Guid serviceTypeId, ServiceTypeModel serviceTypeModel);
    }
}
