using Application.Commons;
using Application.ViewModels.CareStepViewModels;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ICareStepService
    {
        Task AddCareSteps(CareStepModel careStepModel);
        Task<Pagination<CareStep>> GetCareStepsByCategoryId(Guid categoryId);
        Task UpdateCareStep(Guid id, CareStepUpdateModel careStepUpdateModel);
        Task DeleteCareStep(Guid id);
    }
}
