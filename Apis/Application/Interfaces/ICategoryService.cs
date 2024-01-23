using Application.ViewModels.CategoryViewModels;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ICategoryService
    {
        Task<List<Category>> GetCategories();
        Task AddCategory(CategoryModel categoryModel);
        Task UpdateCategory(Guid id, CategoryModel categoryModel);
        Task DeleteCategory(Guid id);
    }
}
