using Application.Commons;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IBonsaiImageService
    {
        Task<Pagination<BonsaiImage>> GetProductImagesByProductId(Guid productId);
        Task AddProductImages(BonsaiImage productImage);
        Task DeleteProductImagesByProductId(Guid productId);
    }
}
