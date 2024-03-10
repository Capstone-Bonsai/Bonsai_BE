using Application.Commons;
using Application.Interfaces;
using Application.ViewModels.CustomerBonsaiViewModels;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class CustomerBonsaiService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IBonsaiService _bonsaiService;

        public CustomerBonsaiService(IUnitOfWork unitOfWork, IMapper mapper, IBonsaiService bonsaiService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _bonsaiService = bonsaiService;
        }
        public async Task AddBonsaiForCustomer(CustomerBonsaiModel customerBonsaiModel, Guid customerId)
        {
            if (customerBonsaiModel == null)
            {
                throw new Exception("Add sai data bonsai/vườn");
            }
            var bonsai = await _unitOfWork.BonsaiRepository.GetByIdAsync(customerBonsaiModel.BonsaiId);
            if (bonsai == null)
            {
                throw new Exception("Không tìm thấy thông tin bonsai");
            }
            var listBoughtBonsai = await _bonsaiService.GetBoughtBonsai(customerId);
            if (!listBoughtBonsai.Items.Contains(bonsai))
            {
                throw new Exception("Cây này không tồn tại trong danh sách đã mua");
            }
            var garden = await _unitOfWork.CustomerGardenRepository.GetByIdAsync(customerBonsaiModel.CustomerGardenId);
            if (garden == null)
            {
                throw new Exception("Không tìm thấy thông tin bonsai");
            }
            var customerBonsai = _mapper.Map<CustomerBonsai>(customerBonsaiModel);
            await _unitOfWork.CustomerBonsaiRepository.AddAsync(customerBonsai);
        }
    }
}
