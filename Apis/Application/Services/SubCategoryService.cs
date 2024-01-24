using Application.Commons;
using Application.Interfaces;
using Application.ViewModels.SubCategoryViewModels;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class SubCategoryService : ISubCategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SubCategoryService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<Pagination<SubCategory>> GetSubCategories()
        {
            var subcategories = await _unitOfWork.SubCategoryRepository.GetAsync(isTakeAll: true, expression: x => !x.IsDeleted, isDisableTracking: true);
            return subcategories;
        }
        public async Task AddSubCategory(SubCategoryModel subCategoryModel)
        {
            var checkCategory = _unitOfWork.SubCategoryRepository.GetAsync(isTakeAll: true, expression: x => x.CategoryId == subCategoryModel.CategoryId && x.Name.ToLower().Equals(subCategoryModel.Name.ToLower()) && !x.IsDeleted, isDisableTracking: true);
            var subCategory = _mapper.Map<SubCategory>(subCategoryModel);
            try
            {
                await _unitOfWork.SubCategoryRepository.AddAsync(subCategory);
                await _unitOfWork.SaveChangeAsync();
            }
            catch (Exception)
            {
                _unitOfWork.SubCategoryRepository.SoftRemove(subCategory);
                throw new Exception("Đã xảy ra lỗi trong quá trình tạo mới. Vui lòng thử lại!");
            }
        }
        public async Task UpdateSubCategory(Guid id, SubCategoryModel subCategoryModel)
        {
            var subCategory = _mapper.Map<SubCategory>(subCategoryModel);
            subCategory.Id = id;
            var result = await _unitOfWork.SubCategoryRepository.GetByIdAsync(subCategory.Id);
            if (result == null)
                throw new Exception("Không tìm thấy sản phẩm!");
            try
            {
                _unitOfWork.SubCategoryRepository.Update(subCategory);
                await _unitOfWork.SaveChangeAsync();
            }
            catch (Exception)
            {
                throw new Exception("Đã xảy ra lỗi trong quá trình cập nhật. Vui lòng thử lại!");
            }
        }
        public async Task DeleteSubCategory(Guid id)
        {
            var result = await _unitOfWork.SubCategoryRepository.GetByIdAsync(id);
            if (result == null)
                throw new Exception("Không tìm thấy phân loại!");
            try
            {
                _unitOfWork.SubCategoryRepository.SoftRemove(result);
                await _unitOfWork.SaveChangeAsync();
            }
            catch (Exception)
            {
                throw new Exception("Đã xảy ra lỗi trong quá trình xóa sản phẩm. Vui lòng thử lại!");
            }
        }
    }
}
