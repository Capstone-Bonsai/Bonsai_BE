using Application.Commons;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class CategoryExpectedPriceService : ICategoryExpectedPriceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CategoryExpectedPriceService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public double GetPrice(float height )
        {
            var price = _unitOfWork.CategoryExpectedPriceRepository.GetExpectedPrice(height);
            return price;

        }
        public async Task<Pagination<CategoryExpectedPrice>> Get()
        {
            var price = await _unitOfWork.CategoryExpectedPriceRepository.GetAsync(isTakeAll: true, expression: x => !x.IsDeleted);
            return price;

        }
    }
}
