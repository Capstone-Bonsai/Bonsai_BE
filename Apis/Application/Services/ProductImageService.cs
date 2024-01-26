using Application.Commons;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;

namespace Application.Services
{
    public class ProductImageService : IProductImageService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProductImageService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<Pagination<ProductImage>> GetProductImagesByProductId(Guid productId)
        {
            var categories = await _unitOfWork.ProductImageRepository.GetAsync(isTakeAll: true, expression: x => !x.IsDeleted && x.ProductId == productId, isDisableTracking: true);
            return categories;
        }
        public async Task AddProductImages(ProductImage productImage)
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
    }
}
