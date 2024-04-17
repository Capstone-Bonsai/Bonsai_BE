﻿using Application.Commons;
using Application.Interfaces;
using Application.Utils;
using Application.ViewModels.ServiceOrderViewModels;
using Application.ViewModels.UserViewModels;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Contracts;

namespace Application.Services
{
    public class ServiceOrderGardenerService : IServiceOrderGardenerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IdUtil _idUtil;

        public ServiceOrderGardenerService(IUnitOfWork unitOfWork, IMapper mapper, UserManager<ApplicationUser> userManager, IdUtil idUtil)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
            _idUtil = idUtil;
        }
        public async Task<Pagination<UserViewModel>> GetGardenerOfServiceOrder(int pageIndex, int pageSize, Guid contractId)
        {
            var contract = await _unitOfWork.ServiceOrderRepository.GetByIdAsync(contractId);
            if (contract == null)
            {
                throw new Exception("Không tìm thấy hợp đồng!");
            }
            var listUser = await _userManager.Users.Where(x => x.Gardener != null && x.Gardener.ContractGardeners.Any(y => y.ServiceOrderId == contractId)).AsNoTracking().OrderBy(x => x.Email).ToListAsync();
            var itemCount = listUser.Count();
            var items = listUser.Skip(pageIndex * pageSize)
                                    .Take(pageSize)
                                    .ToList();
            var result = new Pagination<ApplicationUser>()
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalItemsCount = itemCount,
                Items = items,
            };
            var paginationList = _mapper.Map<Pagination<UserViewModel>>(result);

            foreach (var item in paginationList.Items)
            {
                var gardener = await _idUtil.GetGardenerAsync(Guid.Parse(item.Id));
                var contracts = _unitOfWork.ServiceOrderRepository
                    .GetAllQueryable()
                    .Where(x => x.StartDate.Date <= contract.EndDate.Date && x.EndDate.Date >= contract.StartDate.Date && x.ServiceOrderGardener.Any(y => y.GardenerId == gardener.Id));
                var user = await _userManager.FindByIdAsync(item.Id);
                var isLockout = await _userManager.IsLockedOutAsync(user);
                var roles = await _userManager.GetRolesAsync(user);
                string role = "";
                if (roles != null && roles.Count > 0)
                {
                    role = roles[0];
                }
                item.Id = gardener.Id.ToString();
                item.IsLockout = isLockout;
                item.Role = role;
            }
            return paginationList;
        }
        public async Task AddGardener(ServiceOrderGardenerModel serviceOrderGardenerModel)
        {
            var serviceOrder = await _unitOfWork.ServiceOrderRepository.GetByIdAsync(serviceOrderGardenerModel.ServiceOrderId);
            if (serviceOrder == null)
            {
                throw new Exception("Không tìm thấy hợp đồng");
            }
            List<ServiceOrderGardener> serviceOrderGardeners = new List<ServiceOrderGardener>();
            foreach (Guid id in serviceOrderGardenerModel.GardenerIds)
            {
                var gardener = await _unitOfWork.GardenerRepository.GetByIdAsync(id);
                if(gardener == null)
                {
                    throw new Exception("Không tìm thấy người được thêm vào");
                }
                var gardeners = await _unitOfWork.ServiceOrderGardenerRepository.GetAsync(isTakeAll: true, expression: x => x.GardenerId == id && x.ServiceOrderId == serviceOrder.Id && !x.IsDeleted);
                if (gardeners.Items.Count == 0)
                {
                    serviceOrderGardeners.Add(new ServiceOrderGardener()
                    {
                        ServiceOrderId = serviceOrder.Id,
                        GardenerId = id,
                        HasRequest = false,
                    });
                }
                else
                {
                    throw new Exception("Dịch vụ này đã thêm đủ người!");
                }
            }
            await _unitOfWork.ServiceOrderGardenerRepository.AddRangeAsync(serviceOrderGardeners);
            serviceOrder.ServiceOrderStatus = Domain.Enums.ServiceOrderStatus.Processing;
            _unitOfWork.ServiceOrderRepository.Update(serviceOrder);
            await _unitOfWork.SaveChangeAsync();
        }
    }
}