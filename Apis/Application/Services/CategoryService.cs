using Application.Commons;
using Application.Interfaces;
using Application.ViewModels.CategoryViewModels;
using Application.ViewModels.ProductViewModels;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CategoryService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<Pagination<Category>> GetCategories()
        {
            var categories = await _unitOfWork.CategoryRepository.GetAsync(isTakeAll: true, expression: x => !x.IsDeleted, isDisableTracking: true);
            return categories;
        }
        public async Task AddCategory(CategoryViewModel categoryModel)
        {
            var checkCategory =  _unitOfWork.CategoryRepository.GetAsync(isTakeAll: true, expression: x => x.Name.ToLower().Equals(categoryModel.Name.ToLower()) && !x.IsDeleted, isDisableTracking: true);
            var category = _mapper.Map<Category>(categoryModel);
            try
            {
                await _unitOfWork.CategoryRepository.AddAsync(category);
                await _unitOfWork.SaveChangeAsync();
            }
            catch (Exception)
            {
                _unitOfWork.CategoryRepository.SoftRemove(category);
                throw new Exception("Đã xảy ra lỗi trong quá trình tạo mới. Vui lòng thử lại!");
            }
        }
        public async Task UpdateCategory(Guid id, CategoryViewModel categoryModel)
        {
            var category = _mapper.Map<Category>(categoryModel);
            category.Id = id;
            var result = await _unitOfWork.CategoryRepository.GetByIdAsync(category.Id);
            if (result == null)
                throw new Exception("Không tìm thấy sản phẩm!");
            try
            {
                _unitOfWork.CategoryRepository.Update(category);
                await _unitOfWork.SaveChangeAsync();
            }
            catch (Exception)
            {
                throw new Exception("Đã xảy ra lỗi trong quá trình cập nhật. Vui lòng thử lại!");
            }
        }
        public async Task DeleteCategory(Guid id)
        {
            var result = await _unitOfWork.CategoryRepository.GetByIdAsync(id);
            if (result == null)
                throw new Exception("Không tìm thấy phân loại!");
            try
            {
                _unitOfWork.CategoryRepository.SoftRemove(result);
                await _unitOfWork.SaveChangeAsync();
            }
            catch (Exception)
            {
                throw new Exception("Đã xảy ra lỗi trong quá trình xóa sản phẩm. Vui lòng thử lại!");
            }
        }
    }
}
