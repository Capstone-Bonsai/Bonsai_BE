using Application.Commons;
using Application.ViewModels.CategoryViewModels;
using Application.ViewModels.StyleViewModels;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface ICategoryService
    {
        Task<Pagination<Category>> GetCategories();
        Task<Category?> GetCategoryById(Guid id);
        Task AddCategory(CategoryModel categoryModel);
        Task UpdateCategory(Guid id, CategoryModel categoryModel);
        Task DeleteCategory(Guid id);
    }
}
