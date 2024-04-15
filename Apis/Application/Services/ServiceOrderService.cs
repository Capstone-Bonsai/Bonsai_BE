using Application.Commons;
using Application.Interfaces;
using Application.Repositories;
using Application.Services.Momo;
using Application.Utils;
using Application.ViewModels.BonsaiViewModel;
using Application.ViewModels.ServiceOrderViewModels;
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
    public class ServiceOrderService : IServiceOrderService
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
        public async Task CreateServiceOrder(ServiceOrderModel serviceOrderModel)
        {
            try
            {
                _unitOfWork.BeginTransaction();
                var _service = await _unitOfWork.ServiceRepository.GetByIdAsync(serviceOrderModel.ServiceId);
                if (_service == null)
                {
                    throw new Exception("Không tìm thấy dịch vụ!");
                }
                var customerGarden = await _unitOfWork.CustomerGardenRepository.GetAllQueryable()
                    .AsNoTracking()
                    .Include(x => x.Customer)
                    .ThenInclude(x => x.ApplicationUser)
                    .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == serviceOrderModel.CustomerGardenId);
                ServiceOrder serviceOrder = new ServiceOrder();
                serviceOrder.CustomerGardenId = serviceOrderModel.CustomerGardenId;
                serviceOrder.ServiceId = serviceOrderModel.ServiceId;
                serviceOrder.CustomerBonsaiId = serviceOrderModel.CustomerBonsaiId;
                serviceOrder.CustomerName = customerGarden.Customer.ApplicationUser.Fullname;
                serviceOrder.Address = customerGarden.Address;
                serviceOrder.CustomerPhoneNumber = customerGarden.Customer.ApplicationUser.PhoneNumber;
                var distance = await _deliveryFeeService.GetDistanse(serviceOrder.Address);
                serviceOrder.Distance = distance.rows[0].elements[0].distance.value;
                serviceOrder.StartDate = serviceOrderModel.StartDate;
                serviceOrder.EndDate = serviceOrderModel.EndDate;
                serviceOrder.GardenSquare = customerGarden.Square;
                serviceOrder.ServiceOrderStatus = ServiceOrderStatus.Pending;
                await _unitOfWork.ServiceOrderRepository.AddAsync(serviceOrder);
                /*if (contract.ServiceType == Domain.Enums.ServiceType.GardenCare)
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
                }*/
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception)
            {
                _unitOfWork.RollbackTransaction();
                throw;
            }
        }

        public async Task UpdateServiceOrder(Guid serviceOrderId, ResponseServiceOrderModel responseServiceOrderModel)
        {
            try
            {
                var serviceOrder = await _unitOfWork.ServiceOrderRepository.GetByIdAsync(serviceOrderId);
                if (serviceOrder == null)
                {
                    throw new Exception("Không tìm thấy đơn đặt hàng dịch vụ!");
                }
                serviceOrder.StartDate = responseServiceOrderModel.StartDate ?? serviceOrder.StartDate;
                serviceOrder.EndDate = responseServiceOrderModel.EndDate ?? serviceOrder.EndDate;
                serviceOrder.TotalPrice = responseServiceOrderModel.TotalPrice;
                serviceOrder.ServiceOrderStatus = ServiceOrderStatus.WaitingForPayment;
                _unitOfWork.ServiceOrderRepository.Update(serviceOrder);
                await _unitOfWork.SaveChangeAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<Pagination<ServiceOrder>> GetServiceOrders(int pageIndex, int pageSize, bool isCustomer, Guid id)
        {
            List<Expression<Func<ServiceOrder, object>>> includes = new List<Expression<Func<ServiceOrder, object>>>{
                                 x => x.ServiceOrderGardener,
                                    };
            if (isCustomer)
            {
                var customer = await _idUtil.GetCustomerAsync(id);
                var serviceOrders = await _unitOfWork.ServiceOrderRepository.GetAsync(pageIndex: pageIndex, pageSize: pageSize, expression: x => x.CustomerGarden.CustomerId == customer.Id, orderBy: x => x.OrderByDescending(contract => contract.ServiceOrderStatus), includes: includes);
                return serviceOrders;
            }
            else
            {
                var serviceOrders = await _unitOfWork.ServiceOrderRepository.GetAsync(pageIndex: pageIndex, pageSize: pageSize, orderBy: x => x.OrderByDescending(contract => contract.ServiceOrderStatus), includes: includes);
                return serviceOrders;
            }
        }

        public async Task<List<ServiceOrderForGardenerViewModel>> GetWorkingCalendar(int month, int year, Guid id)
        {
            var gardener = await _idUtil.GetGardenerAsync(id);
            if (gardener == null)
            {
                throw new Exception("Không tìm thấy gardener!");
            }
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            List<ServiceOrderForGardenerViewModel> serviceOrderForGardenerViewModels = new List<ServiceOrderForGardenerViewModel>();
            var serviceOrders = await _unitOfWork.ServiceOrderRepository
                .GetAllQueryable()
                .Where(x => x.ServiceOrderGardener.Any(y => y.GardenerId == gardener.Id && !y.IsDeleted) && x.StartDate <= endDate && x.EndDate >= startDate)
                .ToListAsync();
            if (serviceOrders.Count == 0)
            {
                return new List<ServiceOrderForGardenerViewModel>();
            }
            foreach (ServiceOrder serviceOrder in serviceOrders)
            {
                if (serviceOrder.ServiceOrderStatus == ServiceOrderStatus.Processing || serviceOrder.ServiceOrderStatus == ServiceOrderStatus.ProcessingComplaint ||
                    serviceOrder.ServiceOrderStatus == ServiceOrderStatus.TaskFinished || serviceOrder.ServiceOrderStatus == ServiceOrderStatus.DoneTaskComplaint ||
                     serviceOrder.ServiceOrderStatus == ServiceOrderStatus.Completed)
                {
                    if(serviceOrder.ServiceOrderStatus == ServiceOrderStatus.ProcessingComplaint)
                    {
                        serviceOrder.EndDate.AddDays(7);
                    }
                    serviceOrderForGardenerViewModels.Add(_mapper.Map<ServiceOrderForGardenerViewModel>(serviceOrder));
                }
                    
            }
            return serviceOrderForGardenerViewModels;
        }
        public async Task<ServiceOrderForGardenerViewModel> GetServiceOrderByIdForGardener(Guid serviceOrderId)
        {
            var serviceOrder = await _unitOfWork.ServiceOrderRepository.GetAllQueryable().Include(x => x.CustomerGarden.Customer).FirstOrDefaultAsync(x => x.Id == serviceOrderId);
            if (serviceOrder == null)
                throw new Exception("Không tồn tại hợp đồng");
            var serviceOrderForGardenerViewModel = _mapper.Map<ServiceOrderForGardenerViewModel>(serviceOrder);
            var customerGardenImage = await _unitOfWork.CustomerGardenImageRepository.GetAsync(isTakeAll: true, expression: x => x.CustomerGardenId == serviceOrder.CustomerGardenId && !x.IsDeleted);
            if (customerGardenImage.Items.Count > 0)
            {
                List<string> images = new List<string>();
                foreach (CustomerGardenImage image in customerGardenImage.Items)
                {
                    images.Add(image.Image);
                }
                serviceOrderForGardenerViewModel.Image = images;
            }
            return serviceOrderForGardenerViewModel;
        }
        public async Task<string> PaymentContract(Guid contractId, string userId)
        {
            //check co phai user ko
            var serviceOrder = await _unitOfWork.ServiceOrderRepository.GetAllQueryable().Include(x => x.CustomerGarden).ThenInclude(x => x.Customer.ApplicationUser).FirstOrDefaultAsync(x => x.Id == contractId);
            if (serviceOrder == null)
                throw new Exception("Không tìm thấy hợp đồng bạn yêu cầu");
           
            // check status
            if(serviceOrder.ServiceOrderStatus != Domain.Enums.ServiceOrderStatus.WaitingForPayment)
                 throw new Exception("Không thể tiến hành thanh toán cho hợp đồng này");


            double totalPrice = Math.Round(serviceOrder.TotalPrice);
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
            string extraData = serviceOrder.Id.ToString();
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
            ServiceOrderStatus serviceOrderStatus = ServiceOrderStatus.WaitingForPayment;
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
                    serviceOrderStatus = ServiceOrderStatus.Paid;
                }
                var serviceOrder = await _unitOfWork.ServiceOrderRepository.GetByIdAsync(contractId);
                if (serviceOrder == null)
                    throw new Exception("Không tìm thấy hợp đồng.");
                var serviceOrderTransaction = new ServiceOrderTransaction();
                serviceOrderTransaction.ServiceOrderId = contractId;
                serviceOrderTransaction.Amount = momo.amount;
                serviceOrderTransaction.IpnURL = IpnUrl;
                serviceOrderTransaction.Information = momo.orderInfo;
                serviceOrderTransaction.PartnerCode = partnerCode;
                serviceOrderTransaction.RedirectUrl = redirectUrl;
                serviceOrderTransaction.RequestId = momo.requestId;
                serviceOrderTransaction.RequestType = "captureWallet";
                serviceOrderTransaction.TransactionStatus = transactionStatus;
                serviceOrderTransaction.PaymentMethod = "MOMO Payment";
                serviceOrderTransaction.contractIdFormMomo = momo.orderId;
                serviceOrderTransaction.OrderType = momo.orderType;
                serviceOrderTransaction.TransId = momo.transId;
                serviceOrderTransaction.ResultCode = momo.resultCode;
                serviceOrderTransaction.Message = momo.message;
                serviceOrderTransaction.PayType = momo.payType;
                serviceOrderTransaction.ResponseTime = momo.responseTime;
                serviceOrderTransaction.ExtraData = momo.extraData;
                // Tạo transaction
                serviceOrderTransaction.Signature = momo.signature;
                await _unitOfWork.ContractTransactionRepository.AddAsync(serviceOrderTransaction);
                //Update Contract Status
                serviceOrder.ServiceOrderStatus = serviceOrderStatus;
                _unitOfWork.ServiceOrderRepository.Update(serviceOrder);
                await _unitOfWork.SaveChangeAsync();

            }
            catch (Exception exx)
            {
                throw new Exception($"tạo Transaction lỗi: {exx.Message}");
            }
        }
        public async Task<OverallServiceOrderViewModel> GetServiceOrderById(Guid serviceOrderId, bool isCustomer, Guid userId)
        {
            List<Expression<Func<ServiceOrder, object>>> includes = new List<Expression<Func<ServiceOrder, object>>>{
                                 x => x.Contract,
                                 x => x.Complaints,
                                    };
            Pagination<ServiceOrder>? serviceOrders;
            if (isCustomer)
            {
                var customer = await _idUtil.GetCustomerAsync(userId);
                serviceOrders = await _unitOfWork.ServiceOrderRepository.GetAsync(isTakeAll: true, expression: x => x.CustomerGarden.CustomerId == customer.Id && x.Id == serviceOrderId, includes: includes);
                if (serviceOrders.Items.Count == 0)
                {
                    throw new Exception("Không tìm thấy đơn đặt hàng dịch vụ");
                }
            }
            else
            {
                serviceOrders = await _unitOfWork.ServiceOrderRepository.GetAsync(isTakeAll: true, expression: x => x.Id == serviceOrderId, includes: includes);
                if (serviceOrders.Items.Count == 0)
                {
                    throw new Exception("Không tìm thấy đơn đặt hàng dịch vụ");
                }
            }
            OverallServiceOrderViewModel overallServiceOrderViewModel = _mapper.Map<OverallServiceOrderViewModel>(serviceOrders.Items[0]);
            overallServiceOrderViewModel.TaskOfServiceOrders = await GetTasksAsync(serviceOrderId, overallServiceOrderViewModel.CustomerBonsaiId != null ? true : false);
            overallServiceOrderViewModel.GardenersOfServiceOrder = await GetGardenerOfServiceOrder(serviceOrderId);
            foreach (Complaint complaint in overallServiceOrderViewModel.Complaints)
            {
                complaint.ComplaintImages = await GetIMageAsync(complaint.Id);
            }
            return overallServiceOrderViewModel;
        }
        private async Task<List<TaskOfServiceOrder>> GetTasksAsync(Guid serviceOrderId, bool isBonsaiCare)
        {
            List<TaskOfServiceOrder> tasks = new List<TaskOfServiceOrder>();
            var serviceOrder = await _unitOfWork.ServiceOrderRepository
                    .GetAllQueryable()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == serviceOrderId);
            if (isBonsaiCare)
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
                List<Expression<Func<BonsaiCareStep, object>>> includes = new List<Expression<Func<BonsaiCareStep, object>>>{
                                 x => x.CareStep
                                    };
                var bonsaiCareSteps = await _unitOfWork.BonsaiCareStepRepository.GetAsync(isTakeAll: true, expression: x => !x.IsDeleted && x.ServiceOrderId == serviceOrderId, includes: includes);
                if (bonsaiCareSteps.Items.Count == 0)
                {
                    throw new Exception("Không tìm thấy bất cứ công việc nào của phân loại này");
                }
                foreach (BonsaiCareStep bonsaiCareStep in bonsaiCareSteps.Items)
                {
                    tasks.Add(new TaskOfServiceOrder()
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
                    .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == serviceOrder.ServiceId);
                if (service != null)
                {
                    List<Expression<Func<GardenCareTask, object>>> includes = new List<Expression<Func<GardenCareTask, object>>>{
                                 x => x.BaseTask
                                    };
                    var gardenCareTasks = await _unitOfWork.GardenCareTaskRepository.GetAsync(isTakeAll: true, expression: x => !x.IsDeleted && x.ServiceOrderId == serviceOrderId, includes: includes);
                    if (gardenCareTasks.Items.Count == 0)
                    {
                        throw new Exception("Không tìm thấy bất cứ công việc nào của phân loại này");
                    }
                    foreach (GardenCareTask gardenCareTask in gardenCareTasks.Items)
                    {
                        tasks.Add(new TaskOfServiceOrder()
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
        }
        private async Task<List<UserViewModel>> GetGardenerOfServiceOrder(Guid serviceOrderId)
        {
            var serviceOrder = await _unitOfWork.ServiceOrderRepository.GetByIdAsync(serviceOrderId);
            if (serviceOrder == null)
            {
                throw new Exception("Không tìm thấy đơn đặt hàng dịch vụ!");
            }
            var listUser = await _userManager.Users.Where(x => x.Gardener != null && x.Gardener.ContractGardeners.Any(y => y.ServiceOrderId == serviceOrder.Id)).AsNoTracking().OrderBy(x => x.Email).ToListAsync();
            var gardenerList = _mapper.Map<List<UserViewModel>>(listUser);

            foreach (var item in gardenerList)
            {
                var gardener = await _idUtil.GetGardenerAsync(Guid.Parse(item.Id));
                var serviceOrders = _unitOfWork.ServiceOrderRepository
                    .GetAllQueryable()
                    .Where(x => x.StartDate.Date <= serviceOrder.EndDate.Date && x.EndDate.Date >= serviceOrder.StartDate.Date && x.ServiceOrderGardener.Any(y => y.GardenerId == gardener.Id));
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
        public async Task AddContractImage(Guid contractId, ServiceOrderImageModel serviceOrderImageModel)
        {
            if (serviceOrderImageModel == null)
                throw new ArgumentNullException(nameof(serviceOrderImageModel), "Vui lòng nhập đúng thông tin!");
            if (serviceOrderImageModel.Image == null || serviceOrderImageModel.Image.Count == 0)
            {
                throw new Exception("Không có hình ảnh!");
            }
            var serviceOrder = await _unitOfWork.ServiceOrderRepository.GetByIdAsync(contractId);
            if (serviceOrder == null)
            {
                throw new Exception("Không tìm thấy hợp đồng!");
            }
            try
            {
                _unitOfWork.BeginTransaction();
                var images = await _unitOfWork.ContractImageRepository.GetAsync(isTakeAll: true, expression: x => x.ServiceOrderId == contractId && !x.IsDeleted, isDisableTracking: true);
                _unitOfWork.ContractImageRepository.SoftRemoveRange(images.Items);
                foreach (var singleImage in serviceOrderImageModel.Image.Select((image, index) => (image, index)))
                {
                    string newImageName = serviceOrder.Id + "_i" + singleImage.index;
                    string folderName = $"serviceOrder/{serviceOrder.Id}/Image";
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
        public async Task UpdateServiceOrderStatus(Guid serviceOrderId, ServiceOrderStatus serviceOrderStatus)
        {
            var serviceOrder = await _unitOfWork.ServiceOrderRepository.GetByIdAsync(serviceOrderId);
            if (serviceOrder == null)
            {
                throw new Exception("Không tìm thấy hợp đồng!");
            }
            serviceOrder.ServiceOrderStatus = serviceOrderStatus;
            _unitOfWork.ServiceOrderRepository.Update(serviceOrder);
            await _unitOfWork.SaveChangeAsync();
        }
    }
}
