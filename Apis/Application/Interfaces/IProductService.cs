using Application.Commons;
using Application.ViewModels.ProductModels;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IProductService
    {
        Task<Pagination<Product>> GetPagination(int pageIndex, int pageSize);
        Task<List<Product>> GetProducts();
        Task<Product?> GetProductById(Guid id);
        Task AddProduct(ProductModel productModel);
        Task UpdateProduct(Guid id, ProductModel productModel);
        Task DeleteProduct(Guid id);
    }
}
