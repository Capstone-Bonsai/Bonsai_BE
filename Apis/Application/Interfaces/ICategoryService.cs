using Application.Commons;
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
        Task<Pagination<Category>> GetCategories();
        Task AddCategory(CategoryViewModel categoryModel);
        Task UpdateCategory(Guid id, CategoryViewModel categoryModel);
        Task DeleteCategory(Guid id);
    }
}
