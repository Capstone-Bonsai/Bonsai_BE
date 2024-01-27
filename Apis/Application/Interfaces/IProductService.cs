using Application.Commons;
using Application.ViewModels.ProductViewModels;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IProductService
    {
        Task<Pagination<Product>> GetPagination(int pageIndex, int pageSize);
        Task<Pagination<Product>> GetProducts();
        Task<Pagination<Product>> GetProductsByFilter(int pageIndex, int pageSize, FilterProductModel filterProductModel);
        Task<Product?> GetProductById(Guid id);
        Task<Guid> AddAsync(ProductModel productModel);
        Task UpdateProduct(Guid id, ProductModel productModel);
        Task DeleteProduct(Guid id);
    }
}
