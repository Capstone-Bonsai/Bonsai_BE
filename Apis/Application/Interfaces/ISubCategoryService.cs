using Application.Commons;
using Application.ViewModels.SubCategoryViewModels;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface ISubCategoryService
    {
        Task<Pagination<SubCategory>> GetSubCategories();
        Task<SubCategory?> GetSubCategoryById(Guid id);
        Task AddSubCategory(SubCategoryModel subCategoryModel);
        Task UpdateSubCategory(Guid id, SubCategoryModel subCategoryModel);
        Task DeleteSubCategory(Guid id);
    }
}
