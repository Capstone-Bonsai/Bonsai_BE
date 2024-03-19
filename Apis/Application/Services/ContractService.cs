using Application.Commons;
using Application.Interfaces;
using Application.Repositories;
using Application.ViewModels.ContractViewModels;
using Application.ViewModels.TaskViewModels;
using AutoMapper;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using Org.BouncyCastle.Math.EC.Multiplier;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class ContractService : IContractService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IDeliveryFeeService _deliveryFeeService;

        public ContractService(IUnitOfWork unitOfWork, IMapper mapper, IDeliveryFeeService deliveryFeeService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _deliveryFeeService = deliveryFeeService;
        }
        public async Task CreateContract(ContractModel contractModel)
        {
            try
            {
                _unitOfWork.BeginTransaction();
                var serviceGarden = await _unitOfWork.ServiceGardenRepository.GetByIdAsync(contractModel.ServiceGardenId);
                if (serviceGarden == null)
                {
                    throw new Exception("Không tìm thấy đơn đăng ký");
                }
                var _service = await _unitOfWork.ServiceRepository.GetByIdAsync(serviceGarden.ServiceId);
                if (_service == null)
                {
                    throw new Exception("Không tìm thấy dịch vụ!");
                }
                var customerGarden = await _unitOfWork.CustomerGardenRepository.GetAllQueryable()
                    .AsNoTracking()
                    .Include(x => x.Customer)
                    .ThenInclude(x => x.ApplicationUser)
                    .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == serviceGarden.CustomerGardenId);
                Contract contract = new Contract();
                contract.ServiceGardenId = serviceGarden.Id;
                contract.CustomerName = customerGarden.Customer.ApplicationUser.Fullname;
                contract.Address = customerGarden.Address;
                contract.CustomerPhoneNumber = customerGarden.Customer.ApplicationUser.PhoneNumber;
                var distance = await _deliveryFeeService.GetDistanse(contract.Address);
                contract.Distance = distance.rows[0].elements[0].distance.value;
                contract.StartDate = contractModel.StartDate;
                contract.EndDate = contractModel.EndDate;
                contract.GardenSquare = customerGarden.Square;
                contract.StandardPrice = serviceGarden.TemporaryPrice ?? 0;
                contract.ServicePrice = contractModel.ServicePrice;
                contract.ServiceType = _service.ServiceType;
                await _unitOfWork.ContractRepository.AddAsync(contract);
                if (contract.ServiceType == Domain.Enums.ServiceType.GardenCare)
                {
                    var service = await _unitOfWork.ServiceRepository.GetAllQueryable()
                        .AsNoTracking()
                        .Include(x => x.ServiceBaseTasks)
                        .ThenInclude(y => y.BaseTask)
                        .FirstOrDefaultAsync(x => !x.IsDisable && x.Id == serviceGarden.ServiceId);
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
                            ContractId = contract.Id
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
                        .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == contract.CustomerBonsaiId);
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
                            ContractId = contract.Id
                        });
                    }
                    await _unitOfWork.BonsaiCareStepRepository.AddRangeAsync(bonsaiCareSteps);
                }
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception)
            {
                _unitOfWork.RollbackTransaction();
                throw;
            }
        }
        public async Task<Pagination<Contract>> GetContracts(int pageIndex, int pageSize)
        {
            var contracts = await _unitOfWork.ContractRepository.GetAsync(pageIndex: pageIndex, pageSize: pageSize);
            return contracts;
        }
        public async Task AddContractGardener(ContractGardenerModel contractGardenerModel)
        {
            var contract = await  _unitOfWork.ContractRepository.GetByIdAsync(contractGardenerModel.ContractId);
            if (contract == null)
            {
                throw new Exception("Không tìm thấy hợp đồng");
            }
            List<ContractGardener> contractGardeners = new List<ContractGardener>();
            foreach (Guid id in contractGardenerModel.GardenerIds)
            {
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
            }
            await _unitOfWork.ContractGardenerRepository.AddRangeAsync(contractGardeners);
            await _unitOfWork.SaveChangeAsync();
        }
    }
}
