﻿using Application.Commons;
using Application.Interfaces;
using Application.ViewModels.ContractViewModels;
using Application.ViewModels.UserViewModels;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Contracts;

namespace Application.Services
{
    public class ContractGardenerService : IContractGardenerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;

        public ContractGardenerService(IUnitOfWork unitOfWork, IMapper mapper, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
        }
        public async Task<Pagination<UserViewModel>> GetGardenerOfContract(int pageIndex, int pageSize, Guid contractId)
        {
            var contract = await _unitOfWork.ContractRepository.GetByIdAsync(contractId);
            if (contract == null)
            {
                throw new Exception("Không tìm thấy hợp đồng!");
            }
            var listUser = await _userManager.Users.Where(x => x.Gardener != null && x.Gardener.ContractGardeners.Any(y => y.ContractId == contractId)).AsNoTracking().OrderBy(x => x.Email).ToListAsync();
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
                var gardener = await GetGardenerAsync(Guid.Parse(item.Id));
                var contracts = _unitOfWork.ContractRepository
                    .GetAllQueryable()
                    .Where(x => x.StartDate.Date <= contract.EndDate.Date && x.EndDate.Date >= contract.StartDate.Date && x.ContractGardeners.Any(y => y.GardenerId == gardener.Id));
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
        public async Task DeleteContractGardener(Guid contractId, Guid gardenerId)
        {
            var contractGardener = await _unitOfWork.ContractGardenerRepository.GetAsync(isTakeAll: true, expression: x => x.GardenerId == gardenerId && x.ContractId == contractId && !x.IsDeleted);
            if (contractGardener == null)
            {
                throw new Exception("Người làm vườn chưa làm cho dịch vụ này");
            }
            _unitOfWork.ContractGardenerRepository.SoftRemove(contractGardener.Items[0]);
            await _unitOfWork.SaveChangeAsync();
        }
        private async Task<Gardener> GetGardenerAsync(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
                throw new Exception("Không tìm thấy người làm vườn!");
            var isGardener = await _userManager.IsInRoleAsync(user, "Gardener");
            if (!isGardener)
                throw new Exception("Chỉ người làm vườn mới có thể thêm vào dự án!");
            var gardener = await _unitOfWork.GardenerRepository.GetAllQueryable().FirstOrDefaultAsync(x => x.UserId.ToLower().Equals(user.Id.ToLower()));
            if (gardener == null)
                throw new Exception("Không tìm thấy thông tin người dùng");
            return gardener;
        }
    }
}