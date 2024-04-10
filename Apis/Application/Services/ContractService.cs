using Application.Commons;
using Application.Interfaces;
using Application.Repositories;
using Application.Services.Momo;
using Application.Utils;
using Application.ViewModels.ContractViewModels;
using Application.ViewModels.TaskViewModels;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Firebase.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Math.EC.Multiplier;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class ContractService : IContractService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IDeliveryFeeService _deliveryFeeService;
        private readonly IConfiguration _configuration;
        private readonly IdUtil _idUtil;

        public ContractService(IConfiguration configuration, IUnitOfWork unitOfWork, IMapper mapper, IDeliveryFeeService deliveryFeeService, IdUtil idUtil)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _deliveryFeeService = deliveryFeeService;
            _configuration = configuration;
            _idUtil = idUtil;
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
                contract.CustomerBonsaiId = serviceGarden.CustomerBonsaiId;
                contract.CustomerName = customerGarden.Customer.ApplicationUser.Fullname;
                contract.Address = customerGarden.Address;
                contract.CustomerPhoneNumber = customerGarden.Customer.ApplicationUser.PhoneNumber;
                var distance = await _deliveryFeeService.GetDistanse(contract.Address);
                contract.Distance = distance.rows[0].elements[0].distance.value;
                contract.StartDate = contractModel.StartDate ?? serviceGarden.StartDate;
                contract.EndDate = contractModel.EndDate ?? serviceGarden.EndDate;
                contract.GardenSquare = customerGarden.Square;
                contract.StandardPrice = contractModel.StandardPrice ?? serviceGarden.TemporaryPrice ?? 0;
                contract.SurchargePrice = contractModel.SurchargePrice ?? serviceGarden.TemporarySurchargePrice ?? 0;
                contract.ServicePrice = contractModel.ServicePrice ?? 0;
                contract.NumberOfGardener = contractModel.NumberOfGardener ?? serviceGarden.TemporaryGardener ?? 2;
                contract.TotalPrice = contract.StandardPrice + contract.SurchargePrice + contract.ServicePrice;
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
        public async Task<Pagination<Contract>> GetContracts(int pageIndex, int pageSize, bool isCustomer, Guid id)
        {
            if (isCustomer)
            {
                var customer = await _idUtil.GetCustomerAsync(id);
                var contracts = await _unitOfWork.ContractRepository.GetAsync(pageIndex: pageIndex, pageSize: pageSize, expression: x => x.ServiceGarden.CustomerGarden.CustomerId == customer.Id, orderBy: x => x.OrderByDescending(contract => contract.ContractStatus));
                return contracts;
            }
            else
            {
                var contracts = await _unitOfWork.ContractRepository.GetAsync(pageIndex: pageIndex, pageSize: pageSize, orderBy: x => x.OrderByDescending(contract => contract.ContractStatus));
                return contracts;
            }    
        }

        public async Task<List<ContractViewModel>> GetWorkingCalendar(int month, int year, Guid id)
        {
            var gardener = await _idUtil.GetGardenerAsync(id);
            if (gardener == null)
            {
                throw new Exception("Không tìm thấy gardener!");
            }
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            List<ContractViewModel> contractViewModels = new List<ContractViewModel>();
            var contracts = await _unitOfWork.ContractRepository
                .GetAllQueryable()
                .Where(x => x.ContractGardeners.Any(y => y.GardenerId == gardener.Id && !y.IsDeleted) && x.StartDate <= endDate && x.EndDate >= startDate)
                .ToListAsync();
            if (contracts.Count == 0)
            {
                return new List<ContractViewModel>();
            }
            foreach (Contract contract in contracts)
            {
                contractViewModels.Add(_mapper.Map<ContractViewModel>(contract));
            }
            return contractViewModels;
        }
        public async Task<List<ContractViewModel>> GetTodayProject(Guid id)
        {
            var gardener = await _idUtil.GetGardenerAsync(id);
            if (gardener == null)
            {
                throw new Exception("Không tìm thấy gardener!");
            }
            List<ContractViewModel> contractViewModels = new List<ContractViewModel>();
            var contracts = await _unitOfWork.ContractRepository
                .GetAllQueryable()
                .Where(x => x.ContractGardeners.Any(y => y.GardenerId == gardener.Id && !y.IsDeleted) && x.StartDate <= DateTime.Today && x.EndDate >= DateTime.Today)
                .ToListAsync();
            if (contracts.Count == 0)
            {
                return new List<ContractViewModel>();
            }
            foreach (Contract contract in contracts)
            {
                contractViewModels.Add(_mapper.Map<ContractViewModel>(contract));
            }
            return contractViewModels;
        }
        public async Task<ContractViewModel> GetContractByIdForGardener(Guid id)
        {
            var contract = await _unitOfWork.ContractRepository.GetAllQueryable().Include(x=>x.ServiceGarden.CustomerGarden.Customer).FirstOrDefaultAsync(x=>x.Id == id);
            if (contract == null)
                throw new Exception("Không tồn tại hợp đồng");
            var contractViewModel = _mapper.Map<ContractViewModel>(contract);
            var serviceGarden = await _unitOfWork.ServiceGardenRepository.GetByIdAsync(contract.ServiceGardenId);
            var customerGardenImage = await _unitOfWork.CustomerGardenImageRepository.GetAsync(isTakeAll: true, expression: x => x.CustomerGardenId == serviceGarden.CustomerGardenId && !x.IsDeleted);
            if (customerGardenImage.Items.Count > 0)
            {
                List<string> images = new List<string>();
                foreach (CustomerGardenImage image in customerGardenImage.Items)
                {
                    images.Add(image.Image);
                }
                contractViewModel.Image = images;
            }
            return contractViewModel;
        }
        public async Task<string> PaymentContract(Guid contractId, string userId)
        {
            //check co phai user ko
            var contract = await _unitOfWork.ContractRepository.GetAllQueryable().Include(x => x.ServiceGarden.CustomerGarden).ThenInclude(x => x.Customer.ApplicationUser).FirstOrDefaultAsync(x => x.Id == contractId);
            if (contract == null)
                throw new Exception("Không tìm thấy hợp đồng bạn yêu cầu");
            if (!contract.ServiceGarden.CustomerGarden.Customer.UserId.ToLower().Equals(userId.ToLower()))
                throw new Exception("Bạn không có quyền truy cập vào hợp đồng này");
            // check status
            /* if(contract.ContractStatus != Domain.Enums.ContractStatus.Waiting)
                 throw new Exception("Không thể tiến hành thanh toán cho hợp đồng này");*/

            // 

            double totalPrice = Math.Round(contract.TotalPrice);
            string endpoint = _configuration["MomoServices:endpoint"];
            string partnerCode = _configuration["MomoServices:partnerCode"];
            string accessKey = _configuration["MomoServices:accessKey"];
            string serectkey = _configuration["MomoServices:secretKey"];
            string orderInfo = "Thanh toán hóa đơn hàng tại Thanh Sơn Garden.";
            string redirectUrl = _configuration["MomoServices:redirectUrl"];
            string ipnUrl = _configuration["MomoServices:serviceIpnUrl"];
            string requestType = "captureWallet";
            string amount = totalPrice.ToString();
            string orderId = Guid.NewGuid().ToString();
            string requestId = Guid.NewGuid().ToString();
            string extraData = contract.Id.ToString();
            //captureWallet
            //Before sign HMAC SHA256 signature
            string rawHash = "accessKey=" + accessKey +
                "&amount=" + amount +
                "&extraData=" + extraData +
                "&ipnUrl=" + ipnUrl +
                "&orderId=" + orderId +
                "&orderInfo=" + orderInfo +
                "&partnerCode=" + partnerCode +
                "&redirectUrl=" + redirectUrl +
                "&requestId=" + requestId +
                "&requestType=" + requestType
            ;
            MoMoSecurity crypto = new MoMoSecurity();
            //sign signature SHA256
            string signature = crypto.signSHA256(rawHash, serectkey);
            //build body json request
            JObject message = new JObject
                 {
                { "partnerCode", partnerCode },
                { "partnerName", "Test" },
                { "storeId", "MomoTestStore1" },
                { "requestId", requestId },
                { "amount", amount },
                { "orderId", orderId },
                { "orderInfo", orderInfo },
                { "redirectUrl", redirectUrl },
                { "ipnUrl", ipnUrl },
                { "lang", "en" },
                { "extraData", extraData },
                { "requestType", requestType },
                { "signature", signature }

                };
            try
            {
                string responseFromMomo = PaymentRequest.sendPaymentRequest(endpoint, message.ToString());

                JObject jmessage = JObject.Parse(responseFromMomo);

                return jmessage.GetValue("payUrl").ToString();
            }
            catch
            {
                throw new Exception("Đã xảy ra lối trong quá trình thanh toán. Vui lòng thanh toán lại sau!");
            }

        }

        public async Task HandleIpnAsync(MomoRedirect momo)
        {
            string accessKey = _configuration["MomoServices:accessKey"];
            string IpnUrl = _configuration["MomoServices:serviceIpnUrl"];
            string redirectUrl = _configuration["MomoServices:redirectUrl"];
            string partnerCode = _configuration["MomoServices:partnerCode"];
            string endpoint = _configuration["MomoServices:endpoint"];

            string rawHash = "accessKey=" + accessKey +
                    "&amount=" + momo.amount +
                    "&extraData=" + momo.extraData +
                    "&message=" + momo.message +
                    "&orderId=" + momo.orderId +
                    "&orderInfo=" + momo.orderInfo +
                    "&orderType=" + momo.orderType +
                    "&partnerCode=" + partnerCode +
                    "&payType=" + momo.payType +
                    "&requestId=" + momo.requestId +
                    "&responseTime=" + momo.responseTime +
                    "&resultCode=" + momo.resultCode +
                    "&transId=" + momo.transId;

            //hash rawData
            MoMoSecurity crypto = new MoMoSecurity();
            string secretKey = _configuration["MomoServices:secretKey"];
            string temp = crypto.signSHA256(rawHash, secretKey);
            TransactionStatus transactionStatus = TransactionStatus.Failed;
            ContractStatus contractStatus = ContractStatus.Waiting;
            //check chữ ký
            if (temp != momo.signature)
                throw new Exception("Sai chữ ký");
            //lấy orderid
            Guid contractId = Guid.Parse(momo.extraData);
            try
            {
                if (momo.resultCode == 0)
                {
                    transactionStatus = TransactionStatus.Success;
                    contractStatus = ContractStatus.Paid;
                }
                var contract = await _unitOfWork.ContractRepository.GetByIdAsync(contractId);
                if (contract == null)
                    throw new Exception("Không tìm thấy hợp đồng.");
                var contractTransaction = new ContractTransaction();
                contractTransaction.ContractId = contractId;
                contractTransaction.Amount = momo.amount;
                contractTransaction.IpnURL = IpnUrl;
                contractTransaction.Information = momo.orderInfo;
                contractTransaction.PartnerCode = partnerCode;
                contractTransaction.RedirectUrl = redirectUrl;
                contractTransaction.RequestId = momo.requestId;
                contractTransaction.RequestType = "captureWallet";
                contractTransaction.TransactionStatus = transactionStatus;
                contractTransaction.PaymentMethod = "MOMO Payment";
                contractTransaction.contractIdFormMomo = momo.orderId;
                contractTransaction.OrderType = momo.orderType;
                contractTransaction.TransId = momo.transId;
                contractTransaction.ResultCode = momo.resultCode;
                contractTransaction.Message = momo.message;
                contractTransaction.PayType = momo.payType;
                contractTransaction.ResponseTime = momo.responseTime;
                contractTransaction.ExtraData = momo.extraData;
                // Tạo transaction
                contractTransaction.Signature = momo.signature;
                await _unitOfWork.ContractTransactionRepository.AddAsync(contractTransaction);
                //Update Contract Status
                contract.ContractStatus = contractStatus;
                _unitOfWork.ContractRepository.Update(contract);
                await _unitOfWork.SaveChangeAsync();

            }
            catch (Exception exx)
            {
                throw new Exception($"tạo Transaction lỗi: {exx.Message}");
            }
        }
        public async Task<Contract> GetContractById(Guid id, bool isCustomer, Guid userId)
        {
            List<Expression<Func<Contract, object>>> includes = new List<Expression<Func<Contract, object>>>{
                                 x => x.ContractImages,
                                    };
            if (isCustomer)
            {
                var customer = await _idUtil.GetCustomerAsync(userId);
                var contracts = await _unitOfWork.ContractRepository.GetAsync(isTakeAll: true, expression: x => x.ServiceGarden.CustomerGarden.CustomerId == customer.Id && x.Id == id, includes: includes);
                if (contracts.Items.Count == 0)
                {
                    throw new Exception("Không tìm thấy contract");
                }
                return contracts.Items[0];
            }
            else
            {
                var contracts = await _unitOfWork.ContractRepository.GetAsync(isTakeAll: true, expression: x => x.Id == id, includes: includes);
                if (contracts.Items.Count == 0)
                {
                    throw new Exception("Không tìm thấy contract");
                }
                return contracts.Items[0];
            }
        }
    }
}
