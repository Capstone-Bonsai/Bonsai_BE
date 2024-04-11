using Application.Commons;
using Application.Interfaces;
using Application.Utils;
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
        private readonly IdUtil _idUtil;

        public ContractGardenerService(IUnitOfWork unitOfWork, IMapper mapper, UserManager<ApplicationUser> userManager, IdUtil idUtil)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
            _idUtil = idUtil;
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
                var gardener = await _idUtil.GetGardenerAsync(Guid.Parse(item.Id));
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
        public async Task ChangeContractGardener(Guid contractId, ChangeGardenerViewModel changeGardenerViewModel)
        {
            var contractGardener = await _unitOfWork.ContractGardenerRepository.GetAsync(isTakeAll: true, expression: x => x.GardenerId == changeGardenerViewModel.OldGardenerIds && x.ContractId == contractId && !x.IsDeleted);
            if (contractGardener == null)
            {
                throw new Exception("Người làm vườn chưa làm cho dịch vụ này");
            }
            var gardener = await _unitOfWork.GardenerRepository.GetByIdAsync(changeGardenerViewModel.NewGardenerIds);
            if (gardener == null)
            {
                throw new Exception("Không tìm thấy người được thêm vào");
            }
            contractGardener.Items[0].GardenerId = changeGardenerViewModel.NewGardenerIds;
            _unitOfWork.ContractGardenerRepository.Update(contractGardener.Items[0]);
            await _unitOfWork.SaveChangeAsync();
        }
        public async Task AddContractGardener(ContractGardenerModel contractGardenerModel)
        {
            var contract = await _unitOfWork.ContractRepository.GetByIdAsync(contractGardenerModel.ContractId);
            if (contract == null)
            {
                throw new Exception("Không tìm thấy hợp đồng");
            }
            if (contract.NumberOfGardener != contractGardenerModel.GardenerIds.Count)
            {
                throw new Exception("Phải thêm đúng " + contract.NumberOfGardener + " người");
            }
            List<ContractGardener> contractGardeners = new List<ContractGardener>();
            foreach (Guid id in contractGardenerModel.GardenerIds)
            {
                var gardener = await _unitOfWork.GardenerRepository.GetByIdAsync(id);
                if(gardener == null)
                {
                    throw new Exception("Không tìm thấy người được thêm vào");
                }
                var contractGardener = await _unitOfWork.ContractGardenerRepository.GetAsync(isTakeAll: true, expression: x => x.GardenerId == id && x.ContractId == contract.Id && !x.IsDeleted);
                if (contractGardener.Items.Count == 0)
                {
                    contractGardeners.Add(new ContractGardener()
                    {
                        ContractId = contract.Id,
                        GardenerId = id,
                        HasRequest = false,
                    });
                }
                else
                {
                    throw new Exception("Dịch vụ này đã thêm đủ người!");
                }
            }
            await _unitOfWork.ContractGardenerRepository.AddRangeAsync(contractGardeners);
            await _unitOfWork.SaveChangeAsync();
        }
    }
}
