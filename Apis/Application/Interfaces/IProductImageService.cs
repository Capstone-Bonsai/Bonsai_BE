using Application.Commons;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IProductImageService
    {
        Task<Pagination<ProductImage>> GetProductImagesByProductId(Guid productId);
        Task AddProductImages(ProductImage productImage);
        Task DeleteProductImagesByProductId(Guid productId);
    }
}
