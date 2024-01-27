﻿using Application.Commons;
using Application.Interfaces;
using Application.Validations.Product;
using Application.Validations.Tag;
using Application.ViewModels.CategoryViewModels;
using Application.ViewModels.ProductViewModels;
using Application.ViewModels.TagViewModels;
using AutoMapper;
using Domain.Entities;
using FluentValidation;

namespace Application.Services
{
    public class TagService : ITagService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public TagService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<Pagination<Tag>> GetTags()
        {
            var tags = await _unitOfWork.TagRepository.GetAsync(isTakeAll: true, expression: x => !x.IsDeleted, isDisableTracking: true);
            return tags;
        }
        public async Task<Tag?> GetTagById(Guid id)
        {
            var tag = await _unitOfWork.TagRepository.GetByIdAsync(id);
            return tag;
        }
        public async Task AddTag(TagModel tagModel)
        {
            if (tagModel == null)
                throw new ArgumentNullException(nameof(tagModel), "Vui lòng nhập thêm thông tin phân loại!");
            var validationRules = new TagModelValidator();
            var resultTagInfo = await validationRules.ValidateAsync(tagModel);
            if (!resultTagInfo.IsValid)
            {
                var errors = resultTagInfo.Errors;
                throw new ValidationException("Xác thực không thành công cho phân loại.", errors);
            }
            var checkTag = await _unitOfWork.TagRepository.GetAsync(isTakeAll: true, expression: x => x.Name.ToLower().Equals(tagModel.Name.ToLower()) && !x.IsDeleted, isDisableTracking: true);
            if (checkTag.Items.Count > 0)
                throw new Exception("Phân loại này đã tồn tại!");
            var tag = _mapper.Map<Tag>(tagModel);
            try
            {
                await _unitOfWork.TagRepository.AddAsync(tag);
                await _unitOfWork.SaveChangeAsync();
            }
            catch (Exception)
            {
                _unitOfWork.TagRepository.SoftRemove(tag);
                throw new Exception("Đã xảy ra lỗi trong quá trình tạo mới. Vui lòng thử lại!");
            }
        }
        public async Task UpdateTag(Guid id, TagModel tagModel)
        {
            var checkTag = await _unitOfWork.TagRepository.GetAsync(isTakeAll: true, expression: x => x.Name.ToLower().Equals(tagModel.Name.ToLower()) && !x.IsDeleted, isDisableTracking: true);
            if (checkTag.Items.Count > 0)
                throw new Exception("Phân loại này đã tồn tại!");
            var tag = _mapper.Map<Tag>(tagModel);
            tag.Id = id;
            var result = await _unitOfWork.TagRepository.GetByIdAsync(tag.Id);
            if (result == null)
                throw new Exception("Không tìm thấy sản phẩm!");
            try
            {
                _unitOfWork.TagRepository.Update(tag);
                await _unitOfWork.SaveChangeAsync();
            }
            catch (Exception)
            {
                throw new Exception("Đã xảy ra lỗi trong quá trình cập nhật. Vui lòng thử lại!");
            }
        }
        public async Task DeleteTag(Guid id)
        {
            var result = await _unitOfWork.TagRepository.GetByIdAsync(id);
            if (result == null)
                throw new Exception("Không tìm thấy phân loại!");
            try
            {
                _unitOfWork.TagRepository.SoftRemove(result);
                await _unitOfWork.SaveChangeAsync();
            }
            catch (Exception)
            {
                throw new Exception("Đã xảy ra lỗi trong quá trình xóa sản phẩm. Vui lòng thử lại!");
            }
        }
    }
}