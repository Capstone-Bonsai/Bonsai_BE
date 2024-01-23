using Application.Commons;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentTime _currentTime;
        private readonly IClaimsService _claims;
        public ProductService(IUnitOfWork unitOfWork, IMapper mapper, ICurrentTime currentTime, IClaimsService claims)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentTime = currentTime;
            _claims = claims;
        }

        public async Task<Pagination<Product>> GetProducts(int pageIndex, int pageSize)
        {
            var products = await _unitOfWork.ProductRepository.ToPagination(pageIndex, pageSize);
            return products;
        }
    }
}
