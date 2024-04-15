using Application.Commons;
using Application.Interfaces;
using Application.Repositories;
using Application.Services.Momo;
using Application.Utils;
using Application.ViewModels.BonsaiViewModel;
using Application.ViewModels.ContractViewModels;
using Application.ViewModels.TaskViewModels;
using Application.ViewModels.UserViewModels;
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
using System.Collections.Immutable;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class ServiceOrderService : IContractService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IDeliveryFeeService _deliveryFeeService;
        private readonly IConfiguration _configuration;
        private readonly IdUtil _idUtil;
        private readonly FirebaseService _fireBaseService;
        private readonly UserManager<ApplicationUser> _userManager;
        public ServiceOrderService(IConfiguration configuration, IUnitOfWork unitOfWork, IMapper mapper,
            IDeliveryFeeService deliveryFeeService, IdUtil idUtil, FirebaseService fireBaseService, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _deliveryFeeService = deliveryFeeService;
            _configuration = configuration;
            _idUtil = idUtil;
            _fireBaseService = fireBaseService;
            _userManager = userManager;
        }
        public async Task CreateServiceOrder(ContractModel contractModel)
        {/*
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
                contract.ContractStatus = ContractStatus.Waiting;
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
                serviceGarden.ServiceGardenStatus = ServiceGardenStatus.StaffAccepted;
                _unitOfWork.ServiceGardenRepository.Update(serviceGarden);
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception)
            {
                _unitOfWork.RollbackTransaction();
                throw;
            }*/
        }
        /*public async Task<Pagination<Contract>> GetContracts(int pageIndex, int pageSize, bool isCustomer, Guid id)
        {
            List<Expression<Func<Contract, object>>> includes = new List<Expression<Func<Contract, object>>>{
                                 x => x.ContractGardeners,
                                    };
            if (isCustomer)
            {
                var customer = await _idUtil.GetCustomerAsync(id);
                var contracts = await _unitOfWork.ContractRepository.GetAsync(pageIndex: pageIndex, pageSize: pageSize, expression: x => x.ServiceGarden.CustomerGarden.CustomerId == customer.Id, orderBy: x => x.OrderByDescending(contract => contract.ContractStatus), includes: includes);
                return contracts;
            }
            else
            {
                var contracts = await _unitOfWork.ContractRepository.GetAsync(pageIndex: pageIndex, pageSize: pageSize, orderBy: x => x.OrderByDescending(contract => contract.ContractStatus), includes: includes);
                return contracts;
            }
        }*/

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
            var contracts = await _unitOfWork.ServiceOrderRepository
                .GetAllQueryable()
                .Where(x => x.ServiceOrderGardener.Any(y => y.GardenerId == gardener.Id && !y.IsDeleted) && x.StartDate <= endDate && x.EndDate >= startDate)
                .ToListAsync();
            if (contracts.Count == 0)
            {
                return new List<ContractViewModel>();
            }
            foreach (ServiceOrder contract in contracts)
            {
                if (contract.ContractStatus == ServiceOrderStatus.Processing || contract.ContractStatus == ServiceOrderStatus.ProcessingComplaint || 
                    contract.ContractStatus == ServiceOrderStatus.TaskFinished || contract.ContractStatus == ServiceOrderStatus.DoneTaskComplaint ||
                     contract.ContractStatus == ServiceOrderStatus.Completed)
                {
                    if(contract.ContractStatus == ServiceOrderStatus.ProcessingComplaint)
                    {
                        contract.EndDate.AddDays(5);
                    }
                    contractViewModels.Add(_mapper.Map<ContractViewModel>(contract));
                }
                    
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
            var contracts = await _unitOfWork.ServiceOrderRepository
                .GetAllQueryable()
                .Where(x => x.ServiceOrderGardener.Any(y => y.GardenerId == gardener.Id && !y.IsDeleted) && x.StartDate <= DateTime.Today && x.EndDate >= DateTime.Today)
                .ToListAsync();
            if (contracts.Count == 0)
            {
                return new List<ContractViewModel>();
            }
            foreach (ServiceOrder contract in contracts)
            {
                contractViewModels.Add(_mapper.Map<ContractViewModel>(contract));
            }
            return contractViewModels;
        }
       /* public async Task<ContractViewModel> GetContractByIdForGardener(Guid id)
        {
            var contract = await _unitOfWork.ContractRepository.GetAllQueryable().Include(x => x.ServiceGarden.CustomerGarden.Customer).FirstOrDefaultAsync(x => x.Id == id);
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
        }*/
        /*public async Task<string> PaymentContract(Guid contractId, string userId)
        {
            //check co phai user ko
            var contract = await _unitOfWork.ContractRepository.GetAllQueryable().Include(x => x.ServiceGarden.CustomerGarden).ThenInclude(x => x.Customer.ApplicationUser).FirstOrDefaultAsync(x => x.Id == contractId);
            if (contract == null)
                throw new Exception("Không tìm thấy hợp đồng bạn yêu cầu");
            if (!contract.ServiceGarden.CustomerGarden.Customer.UserId.ToLower().Equals(userId.ToLower()))
                throw new Exception("Bạn không có quyền truy cập vào hợp đồng này");
            // check status
            *//* if(contract.ContractStatus != Domain.Enums.ContractStatus.Waiting)
                 throw new Exception("Không thể tiến hành thanh toán cho hợp đồng này");*//*

            // 

            double totalPrice = Math.Round(contract.TotalPrice);
            string endpoint = _configuration["MomoServices:endpoint"];
            string partnerCode = _configuration["MomoServices:partnerCode"];
            string accessKey = _configuration["MomoServices:accessKey"];
            string serectkey = _configuration["MomoServices:secretKey"];
            string orderInfo = "Thanh toán hóa đơn hàng tại Thanh Sơn Garden.";
            string redirectUrl = _configuration["MomoServices:redirectContractUrl"];
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
*/
        public async Task HandleIpnAsync(MomoRedirect momo)
        {
            string accessKey = _configuration["MomoServices:accessKey"];
            string IpnUrl = _configuration["MomoServices:serviceIpnUrl"];
            string redirectUrl = _configuration["MomoServices:redirectContractUrl"];
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
            ServiceOrderStatus contractStatus = ServiceOrderStatus.Waiting;
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
                    contractStatus = ServiceOrderStatus.Paid;
                }
                var contract = await _unitOfWork.ServiceOrderRepository.GetByIdAsync(contractId);
                if (contract == null)
                    throw new Exception("Không tìm thấy hợp đồng.");
                var contractTransaction = new ServiceOrderTransaction();
                contractTransaction.ServiceOrderId = contractId;
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
                _unitOfWork.ServiceOrderRepository.Update(contract);
                await _unitOfWork.SaveChangeAsync();

            }
            catch (Exception exx)
            {
                throw new Exception($"tạo Transaction lỗi: {exx.Message}");
            }
        }
       /* public async Task<OverallContractViewModel> GetContractById(Guid id, bool isCustomer, Guid userId)
        {
            List<Expression<Func<Contract, object>>> includes = new List<Expression<Func<Contract, object>>>{
                                 x => x.ContractImages,
                                 x => x.Complaints,
                                    };
            Pagination<Contract>? contracts;
            if (isCustomer)
            {
                var customer = await _idUtil.GetCustomerAsync(userId);
                contracts = await _unitOfWork.ContractRepository.GetAsync(isTakeAll: true, expression: x => x.ServiceGarden.CustomerGarden.CustomerId == customer.Id && x.Id == id, includes: includes);
                if (contracts.Items.Count == 0)
                {
                    throw new Exception("Không tìm thấy contract");
                }
            }
            else
            {
                contracts = await _unitOfWork.ContractRepository.GetAsync(isTakeAll: true, expression: x => x.Id == id, includes: includes);
                if (contracts.Items.Count == 0)
                {
                    throw new Exception("Không tìm thấy contract");
                }
            }
            OverallContractViewModel overallContractViewModel = _mapper.Map<OverallContractViewModel>(contracts.Items[0]);
            overallContractViewModel.TaskOfContracts = await GetTasksAsync(id, overallContractViewModel.ServiceType == ServiceType.BonsaiCare ? true : false);
            overallContractViewModel.GardenersOfContract = await GetGardenerOfContract(id);
            foreach (Complaint complaint in overallContractViewModel.Complaints)
            {
                complaint.ComplaintImages = await GetIMageAsync(complaint.Id);
            }
            return overallContractViewModel;
        }*/
       /* private async Task<List<TaskOfContract>> GetTasksAsync(Guid contractId, bool isBonsaiCare)
        {
            List<TaskOfContract> tasks = new List<TaskOfContract>();
            var contract = await _unitOfWork.ContractRepository
                    .GetAllQueryable()
                    .AsNoTracking()
                    .Include(x => x.ServiceGarden)
                    .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == contractId);
            if (isBonsaiCare)
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
                List<Expression<Func<BonsaiCareStep, object>>> includes = new List<Expression<Func<BonsaiCareStep, object>>>{
                                 x => x.CareStep
                                    };
                var bonsaiCareSteps = await _unitOfWork.BonsaiCareStepRepository.GetAsync(isTakeAll: true, expression: x => !x.IsDeleted && x.ContractId == contractId, includes: includes);
                if (bonsaiCareSteps.Items.Count == 0)
                {
                    throw new Exception("Không tìm thấy bất cứ công việc nào của phân loại này");
                }
                foreach (BonsaiCareStep bonsaiCareStep in bonsaiCareSteps.Items)
                {
                    tasks.Add(new TaskOfContract()
                    {
                        Id = bonsaiCareStep.Id,
                        Name = bonsaiCareStep.CareStep.Step,
                        CompletedTime = bonsaiCareStep.CompletedTime,
                        Note = bonsaiCareStep.Note,
                    });
                }
            }
            else
            {
                Service? service = new Service();
                service = await _unitOfWork.ServiceRepository
                    .GetAllQueryable()
                    .AsNoTracking()
                    .Include(x => x.ServiceBaseTasks)
                .ThenInclude(x => x.BaseTask)
                    .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == contract.ServiceGarden.ServiceId);
                if (service != null)
                {
                    List<Expression<Func<GardenCareTask, object>>> includes = new List<Expression<Func<GardenCareTask, object>>>{
                                 x => x.BaseTask
                                    };
                    var gardenCareTasks = await _unitOfWork.GardenCareTaskRepository.GetAsync(isTakeAll: true, expression: x => !x.IsDeleted && x.ContractId == contractId, includes: includes);
                    if (gardenCareTasks.Items.Count == 0)
                    {
                        throw new Exception("Không tìm thấy bất cứ công việc nào của phân loại này");
                    }
                    foreach (GardenCareTask gardenCareTask in gardenCareTasks.Items)
                    {
                        tasks.Add(new TaskOfContract()
                        {
                            Id = gardenCareTask.Id,
                            Name = gardenCareTask.BaseTask.Name,
                            CompletedTime = gardenCareTask.CompletedTime,
                            Note = gardenCareTask.Note,
                        });
                    }
                }
                else
                {
                    throw new Exception("Không tìm thấy dịch vụ");
                }
            }
            return tasks;
        }*/
        private async Task<List<UserViewModel>> GetGardenerOfContract(Guid contractId)
        {
            var contract = await _unitOfWork.ServiceOrderRepository.GetByIdAsync(contractId);
            if (contract == null)
            {
                throw new Exception("Không tìm thấy hợp đồng!");
            }
            var listUser = await _userManager.Users.Where(x => x.Gardener != null && x.Gardener.ContractGardeners.Any(y => y.ServiceOrderId == contractId)).AsNoTracking().OrderBy(x => x.Email).ToListAsync();
            var gardenerList = _mapper.Map<List<UserViewModel>>(listUser);

            foreach (var item in gardenerList)
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
            return gardenerList;
        }
        private async Task<List<ComplaintImage>> GetIMageAsync(Guid complaintId)
        {

            var imageList = await _unitOfWork.ComplaintImageRepository.GetAsync(isTakeAll: true, expression: x => x.ComplaintId == complaintId && !x.IsDeleted);
            if (imageList.Items.Count == 0)
            {
                return new List<ComplaintImage>();
            }
            return imageList.Items;
        }
        public async Task AddContractImage(Guid contractId, ContractImageModel contractImageModel)
        {
            if (contractImageModel == null)
                throw new ArgumentNullException(nameof(contractImageModel), "Vui lòng nhập đúng thông tin!");
            if (contractImageModel.Image == null || contractImageModel.Image.Count == 0)
            {
                throw new Exception("Không có hình ảnh!");
            }
            var contract = await _unitOfWork.ServiceOrderRepository.GetByIdAsync(contractId);
            if (contract == null)
            {
                throw new Exception("Không tìm thấy hợp đồng!");
            }
            try
            {
                _unitOfWork.BeginTransaction();
                var images = await _unitOfWork.ContractImageRepository.GetAsync(isTakeAll: true, expression: x => x.ServiceOrderId == contractId && !x.IsDeleted, isDisableTracking: true);
                _unitOfWork.ContractImageRepository.SoftRemoveRange(images.Items);
                foreach (var singleImage in contractImageModel.Image.Select((image, index) => (image, index)))
                {
                    string newImageName = contract.Id + "_i" + singleImage.index;
                    string folderName = $"contract/{contract.Id}/Image";
                    string imageExtension = Path.GetExtension(singleImage.image.FileName);
                    string[] validImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
                    const long maxFileSize = 20 * 1024 * 1024;
                    if (Array.IndexOf(validImageExtensions, imageExtension.ToLower()) == -1 || singleImage.image.Length > maxFileSize)
                    {
                        throw new Exception("Có chứa file không phải ảnh hoặc quá dung lượng tối đa(>20MB)!");
                    }
                    var url = await _fireBaseService.UploadFileToFirebaseStorage(singleImage.image, newImageName, folderName);
                    if (url == null)
                        throw new Exception("Lỗi khi đăng ảnh lên firebase!");

                    Contract contractImage = new Contract()
                    {
                        ServiceOrderId = contractId,
                        Image = url
                    };

                    await _unitOfWork.ContractImageRepository.AddAsync(contractImage);
                }
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task UpdateContractStatus(Guid contractId, ServiceOrderStatus contractStatus)
        {
           /* var contract = await _unitOfWork.ContractRepository.GetByIdAsync(contractId);
            if (contract == null)
            {
                throw new Exception("Không tìm thấy hợp đồng!");
            }
            contract.ContractStatus = contractStatus;
            _unitOfWork.ContractRepository.Update(contract);
            if(contractStatus == ContractStatus.Completed)
            {
                var serviceGarden = await _unitOfWork.ServiceGardenRepository.GetByIdAsync(contract.ServiceGardenId);
                if (serviceGarden == null)
                {
                    throw new Exception("Không tìm thấy đơn đăng ký!");
                }
                serviceGarden.ServiceGardenStatus = ServiceGardenStatus.Finished;
                _unitOfWork.ServiceGardenRepository.Update(serviceGarden);
            }
            await _unitOfWork.SaveChangeAsync();*/
        }
    }
}
