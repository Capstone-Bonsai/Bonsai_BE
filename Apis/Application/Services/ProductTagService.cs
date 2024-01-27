using Application.Commons;
using Application.Interfaces;
using Application.Validations.Product;
using Application.ViewModels.ProductTagViewModels;
using Application.ViewModels.ProductViewModels;
using Application.ViewModels.SubCategoryViewModels;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class ProductTagService : IProductTagService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProductTagService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task AddAsync(ProductTagModel productTagModel)
        {
            var product = await _unitOfWork.ProductRepository.GetByIdAsync(productTagModel.ProductId);
            if (product == null)
                throw new Exception("Không tìm thấy sản phẩm!");
            var tag = await _unitOfWork.TagRepository.GetByIdAsync(productTagModel.TagId);
            if (tag == null)
                throw new Exception("Không tìm thấy phân loại!");
            var productTag = _mapper.Map<ProductTag>(productTagModel);
            await _unitOfWork.ProductTagRepository.AddAsync(productTag);
        }
        public async Task SoftDeleteAsync(ProductTagModel productTagModel)
        {
            var productTag = await _unitOfWork.ProductTagRepository.GetAsync(isTakeAll: true, expression: x => x.ProductId == productTagModel.ProductId && x.TagId == productTagModel.TagId && !x.IsDeleted, isDisableTracking: true);
            if (productTag.Items.Count == 0)
                throw new Exception("Không tìm thấy!");
            _unitOfWork.ProductTagRepository.SoftRemoveRange(productTag.Items);
        }
    }
}
