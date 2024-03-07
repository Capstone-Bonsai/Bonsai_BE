using Application.Commons;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;

namespace Application.Services
{
    public class BonsaiImageService : IBonsaiImageService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public BonsaiImageService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<Pagination<BonsaiImage>> GetProductImagesByProductId(Guid bonsaiId)
        {
            var categories = await _unitOfWork.ProductImageRepository.GetAsync(isTakeAll: true, expression: x => !x.IsDeleted && x.BonsaiId == bonsaiId, isDisableTracking: true);
            return categories;
        }
        public async Task AddProductImages(BonsaiImage productImage)
        {
            try
            {
                await _unitOfWork.ProductImageRepository.AddAsync(productImage);
                await _unitOfWork.SaveChangeAsync();
            }
            catch (Exception)
            {
                _unitOfWork.ProductImageRepository.SoftRemove(productImage);
                throw new Exception("Đã xảy ra lỗi trong quá trình tạo mới. Vui lòng thử lại!");
            }
        }
        public async Task DeleteProductImagesByProductId(Guid bonsaiId)
        {
            try
            {
                var productImages = await _unitOfWork.ProductImageRepository.GetAsync(isTakeAll: true, expression: x => !x.IsDeleted && x.BonsaiId == bonsaiId, isDisableTracking: true);
                _unitOfWork.ProductImageRepository.SoftRemoveRange(productImages.Items);
                await _unitOfWork.SaveChangeAsync();
            }
            catch (Exception)
            {
                throw new Exception("Đã xảy ra lỗi trong quá trình xóa ảnh. Vui lòng thử lại!");
            }
        }
    }
}
