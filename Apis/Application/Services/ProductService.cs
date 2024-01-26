using Application.Commons;
using Application.Interfaces;
using Application.ViewModels.ProductViewModels;
using AutoMapper;
using Domain.Entities;

namespace Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProductService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Pagination<Product>> GetPagination(int pageIndex, int pageSize)
        {
            var products = await _unitOfWork.ProductRepository.GetAsync(pageIndex: pageIndex, pageSize: pageSize);
            return products;
        }
        public async Task<Pagination<Product>> GetProducts()
        {
            var products = await _unitOfWork.ProductRepository.GetAsync(isTakeAll: true, expression: x => !x.IsDeleted, isDisableTracking: true);
            return products;
        }

        public async Task<Product?> GetProductById(Guid id)
        {
            var product = await _unitOfWork.ProductRepository.GetByIdAsync(id);
            return product;
        }

        public async Task<Guid> AddAsyncGetId(ProductModel productModel)
        {
            var product = _mapper.Map<Product>(productModel);
            try
            {
                await _unitOfWork.ProductRepository.AddAsync(product);
                await _unitOfWork.SaveChangeAsync();
                return product.Id;
            }
            catch (Exception)
            {
                _unitOfWork.ProductRepository.SoftRemove(product);
                throw new Exception("Đã xảy ra lỗi trong quá trình tạo mới. Vui lòng thử lại!");
            }
        }
        public async Task UpdateProduct(Guid id, ProductModel productModel)
        {
            var product = _mapper.Map<Product>(productModel);
            product.Id = id;
            var result = await _unitOfWork.ProductRepository.GetByIdAsync(product.Id);
            if (result == null)
                throw new Exception("Không tìm thấy sản phẩm!");
            try
            {
                _unitOfWork.ProductRepository.Update(product);
                await _unitOfWork.SaveChangeAsync();
            }
            catch (Exception)
            {
                throw new Exception("Đã xảy ra lỗi trong quá trình cập nhật. Vui lòng thử lại!");
            }
        }
        public async Task DeleteProduct(Guid id)
        {
            var result = await _unitOfWork.ProductRepository.GetByIdAsync(id);
            if (result == null)
                throw new Exception("Không tìm thấy sản phẩm!");
            try
            {
                _unitOfWork.ProductRepository.SoftRemove(result);
                await _unitOfWork.SaveChangeAsync();
            }
            catch (Exception)
            {
                throw new Exception("Đã xảy ra lỗi trong quá trình xóa sản phẩm. Vui lòng thử lại!");
            }
        }
    }
}
