using Application.Commons;
using Application.Interfaces;
using Application.ViewModels.TagViewModels;
using AutoMapper;
using Domain.Entities;

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
        public async Task AddTag(TagModel tagModel)
        {
            var checkTag = _unitOfWork.TagRepository.GetAsync(isTakeAll: true, expression: x => x.Name.ToLower().Equals(tagModel.Name.ToLower()) && !x.IsDeleted, isDisableTracking: true);
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
