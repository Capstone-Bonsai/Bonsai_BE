using Application.Commons;
using Application.ViewModels.ProductViewModels;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IProductService
    {
        Task<Pagination<Product>> GetPagination(int pageIndex, int pageSize, bool isAdmin = false);
        Task<Pagination<Product>> GetProducts(bool isAdmin = false);
        Task<Pagination<Product>> GetProductsByFilter(int pageIndex, int pageSize, FilterProductModel filterProductModel, bool isAdmin = false);
        Task<Product?> GetProductById(Guid id, bool isAdmin = false);
        Task<Guid> AddAsync(ProductModel productModel);
        Task UpdateProduct(Guid id, ProductModel productModel);
        Task DeleteProduct(Guid id);
        Task UpdateProductAvailability(Guid id);
    }
}
