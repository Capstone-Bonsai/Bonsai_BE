﻿using Application.Commons;
using Application.ViewModels.ProductViewModels;
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
        Task<Pagination<Product>> GetProducts();
        Task<Product?> GetProductById(Guid id);
        Task<Guid> AddAsyncGetId(ProductModel productModel);
        Task UpdateProduct(Guid id, ProductModel productModel);
        Task DeleteProduct(Guid id);
    }
}