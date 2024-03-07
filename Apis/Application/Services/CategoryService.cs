using Application.Commons;
using Application.Interfaces;
using Application.ViewModels.CategoryViewModels;
using AutoMapper;
using Domain.Entities;
using System.Linq.Expressions;

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
        public async Task AddCategory(CategoryModel categoryModel)
        {
            var checkCategory = await _unitOfWork.CategoryRepository.GetAsync(isTakeAll: true, expression: x => x.Name.ToLower().Equals(categoryModel.Name.ToLower()) && !x.IsDeleted, isDisableTracking: true);
            if (checkCategory.Items.Count > 0)
                throw new Exception("Phân loại này đã tồn tại!");
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
        public async Task<Category?> GetCategoryById(Guid id)
        {
            var category = await _unitOfWork.CategoryRepository.GetByIdAsync(id);
            return category;
        }
        public async Task UpdateCategory(Guid id, CategoryModel categoryModel)
        {
            var checkCategory = await _unitOfWork.CategoryRepository.GetAsync(isTakeAll: true, expression: x => x.Name.ToLower().Equals(categoryModel.Name.ToLower()) && !x.IsDeleted, isDisableTracking: true);
            if (checkCategory.Items.Count > 0)
                throw new Exception("Phân loại này đã tồn tại!");
            var category = _mapper.Map<Category>(categoryModel);
            category.Id = id;
            var result = await _unitOfWork.CategoryRepository.GetByIdAsync(category.Id);
            if (result == null)
                throw new Exception("Không tìm thấy phân loại!");
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
