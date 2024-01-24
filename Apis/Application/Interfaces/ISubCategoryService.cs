using Application.Commons;
using Application.ViewModels.SubCategoryViewModels;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ISubCategoryService
    {
        Task<Pagination<SubCategory>> GetSubCategories();
        Task AddSubCategory(SubCategoryModel subCategoryModel);
        Task UpdateSubCategory(Guid id, SubCategoryModel subCategoryModel);
        Task DeleteSubCategory(Guid id);
    }
}
