using Application.Commons;
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
            try
            {
                _unitOfWork.BeginTransaction();
                bool firstAdd = false;
                var serviceOrderGardener = await _unitOfWork.ServiceOrderGardenerRepository.GetAsync(isTakeAll: true, expression: x => x.ServiceOrderId == serviceOrder.Id && !x.IsDeleted);
                if (serviceOrderGardener.Items.Count == 0)
                {
                    firstAdd = true;
                }
                List<ServiceOrderGardener> serviceOrderGardeners = new List<ServiceOrderGardener>();
                foreach (Guid id in serviceOrderGardenerModel.GardenerIds)
                {
                    var gardener = await _unitOfWork.GardenerRepository.GetByIdAsync(id);
                    if (gardener == null)
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
                if (firstAdd)
                {
                    await AddTaskForServiceOrder(serviceOrder);
                }
                serviceOrder.ServiceOrderStatus = Domain.Enums.ServiceOrderStatus.Processing;
                _unitOfWork.ServiceOrderRepository.Update(serviceOrder);
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                _unitOfWork.RollbackTransaction();
                throw new Exception(ex.Message);
            }
        }
        private async Task AddTaskForServiceOrder(ServiceOrder serviceOrder)
        {
            if (serviceOrder.CustomerBonsaiId == null)
                {
                    var service = await _unitOfWork.ServiceRepository.GetAllQueryable()
                        .AsNoTracking()
                        .Include(x => x.ServiceBaseTasks)
                        .ThenInclude(y => y.BaseTask)
                        .FirstOrDefaultAsync(x => !x.IsDisable && x.Id == serviceOrder.ServiceId);
                    if (service == null)
                    {
                        throw new Exception("Không tìm thấy dịch vụ");
                    }
                    List<GardenCareTask> gardenCareTasks = new List<GardenCareTask>();
                    foreach (ServiceBaseTask serviceBaseTask in service.ServiceBaseTasks)
                    {
                        BaseTask baseTask = serviceBaseTask.BaseTask;
                        gardenCareTasks.Add(new GardenCareTask()
                        {
                            BaseTaskId = baseTask.Id,
                            ServiceOrderId = serviceOrder.Id
                        });
                    }
                    await _unitOfWork.GardenCareTaskRepository.AddRangeAsync(gardenCareTasks);
                }
                else
                {
                    CustomerBonsai? customerBonsai = new CustomerBonsai();
                    customerBonsai = await _unitOfWork.CustomerBonsaiRepository
                        .GetAllQueryable()
                        .AsNoTracking()
                        .Include(x => x.Bonsai)
                        .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == serviceOrder.CustomerBonsaiId);
                    if (customerBonsai == null)
                    {
                        throw new Exception("Không tìm thấy bonsai");
                    }
                    var careSteps = await _unitOfWork.CareStepRepository.GetAsync(isTakeAll: true, expression: x => x.CategoryId == customerBonsai.Bonsai.CategoryId && !x.IsDeleted);
                    List<BonsaiCareStep> bonsaiCareSteps = new List<BonsaiCareStep>();
                    foreach (CareStep careStep in careSteps.Items)
                    {
                        bonsaiCareSteps.Add(new BonsaiCareStep()
                        {
                            CareStepId = careStep.Id,
                            ServiceOrderId = serviceOrder.Id
                        });
                    }
                    await _unitOfWork.BonsaiCareStepRepository.AddRangeAsync(bonsaiCareSteps);
                }
        }
    }
}
