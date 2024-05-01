using Application.Commons;
using Application.Interfaces;
using Application.Repositories;
using Application.Services.Momo;
using Application.Utils;
using Application.Validations.Bonsai;
using Application.Validations.ServiceOrder;
using Application.ViewModels.BonsaiViewModel;
using Application.ViewModels.ServiceOrderViewModels;
using Application.ViewModels.TaskViewModels;
using Application.ViewModels.UserViewModels;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Firebase.Auth;
using Microsoft.AspNetCore.Http;
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
        private readonly INotificationService _notificationService;
        public ServiceOrderService(IConfiguration configuration, IUnitOfWork unitOfWork, IMapper mapper,
            IDeliveryFeeService deliveryFeeService, IdUtil idUtil, FirebaseService fireBaseService, UserManager<ApplicationUser> userManager,
            INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _deliveryFeeService = deliveryFeeService;
            _configuration = configuration;
            _idUtil = idUtil;
            _fireBaseService = fireBaseService;
            _userManager = userManager;
            _notificationService = notificationService;
        }
        public async Task CreateServiceOrder(ServiceOrderModel serviceOrderModel)
        {
            var validationRules = new ServiceOrderModelValidator();
            var resultServiceOrderInfo = await validationRules.ValidateAsync(serviceOrderModel);
            if (!resultServiceOrderInfo.IsValid)
            {
                var errors = resultServiceOrderInfo.Errors.Select(x => x.ErrorMessage);
                string errorMessage = string.Join(Environment.NewLine, errors);
                throw new Exception(errorMessage);
            }
            
            try
            {
                _unitOfWork.BeginTransaction();
                var _service = await _unitOfWork.ServiceRepository.GetByIdAsync(serviceOrderModel.ServiceId);
                if (_service == null)
                {
                    throw new Exception("Không tìm thấy dịch vụ!");
                }
                if (serviceOrderModel.CustomerGardenId == null && serviceOrderModel.CustomerBonsaiId == null)
                {
                    throw new Exception("Vui lòng nhập vườn/bonsai cần chăm sóc!");
                }
                var service = await _unitOfWork.ServiceRepository.GetAllQueryable()
.AsNoTracking()
.Include(x => x.ServiceType)
.FirstOrDefaultAsync(x => !x.IsDeleted && !x.IsDisable && x.Id == serviceOrderModel.ServiceId);
                if (serviceOrderModel.CustomerGardenId == null)
                {
                    var _serviceOrder = await _unitOfWork.ServiceOrderRepository.GetAsync(isTakeAll: true, expression: x => x.CustomerBonsaiId == serviceOrderModel.CustomerBonsaiId && (x.ServiceOrderStatus != ServiceOrderStatus.Fail && x.ServiceOrderStatus != ServiceOrderStatus.Completed && x.ServiceOrderStatus != ServiceOrderStatus.Canceled));
                    if (_serviceOrder.Items != null && _serviceOrder.Items.Count != 0)
                    {
                        throw new Exception("Đang có đơn hàng dịch vụ thuộc bonsai này chưa hoàn thành, vui lòng chọn cây khác!");
                    }
                    if (service.ServiceType.TypeEnum != TypeEnum.Bonsai)
                    {
                        throw new Exception("Vui lòng chọn dịch vụ bonsai!");
                    }
                    if (serviceOrderModel.CustomerBonsaiId == null)
                    {
                        throw new Exception("");
                    }
                    var customerBonsai = await _unitOfWork.CustomerBonsaiRepository.GetByIdAsync(serviceOrderModel.CustomerBonsaiId.Value);
                    if (customerBonsai == null)
                    {
                        throw new Exception("Không tìm thấy cây này!");
                    }
                    serviceOrderModel.CustomerGardenId = customerBonsai.CustomerGardenId;
                }
                else
                {
                    if (service.ServiceType.TypeEnum != TypeEnum.Garden)
                    {
                        throw new Exception("Vui lòng chọn dịch vụ vườn!");
                    }
                    var _serviceOrder = await _unitOfWork.ServiceOrderRepository.GetAsync(isTakeAll: true, expression: x => x.CustomerGardenId == serviceOrderModel.CustomerGardenId && (x.ServiceOrderStatus != ServiceOrderStatus.Fail && x.ServiceOrderStatus != ServiceOrderStatus.Completed && x.ServiceOrderStatus != ServiceOrderStatus.Canceled));
                    if (_serviceOrder.Items != null && _serviceOrder.Items.Count != 0)
                    {
                        throw new Exception("Đang có đơn hàng dịch vụ thuộc vườn này chưa hoàn thành, vui lòng chọn vườn khác!");
                    }
                    var _customerGarden = await _unitOfWork.CustomerGardenRepository.GetByIdAsync(serviceOrderModel.CustomerGardenId.Value);
                    if (_customerGarden == null)
                    {
                        throw new Exception("Không tìm thấy vườn này!");
                    }
                }
                var customerGarden = await _unitOfWork.CustomerGardenRepository.GetAllQueryable()
                    .AsNoTracking()
                    .Include(x => x.Customer)
                    .ThenInclude(x => x.ApplicationUser)
                    .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == serviceOrderModel.CustomerGardenId);
                ServiceOrder serviceOrder = new ServiceOrder();
                serviceOrder.CustomerGardenId = serviceOrderModel.CustomerGardenId.Value;
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
                await _unitOfWork.CommitTransactionAsync();
                await _notificationService.SendToStaff("Có đơn hàng dịch vụ mới", $"{serviceOrder.CustomerName} đã đăng ký đơn hàng dịch vụ thành công!");
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
                                 x => x.Service,
                                 x => x.Contract
                                    };
            if (isCustomer)
            {
                var customer = await _idUtil.GetCustomerAsync(id);
                var serviceOrders = await _unitOfWork.ServiceOrderRepository.GetAsync(pageIndex: pageIndex, pageSize: pageSize, expression: x => x.CustomerGarden.CustomerId == customer.Id, orderBy: x => x.OrderByDescending(contract => contract.CreationDate), includes: includes);
                return serviceOrders;
            }
            else
            {
                var serviceOrders = await _unitOfWork.ServiceOrderRepository.GetAsync(pageIndex: pageIndex, pageSize: pageSize,
                    orderBy: x => x.OrderBy(order => order.ServiceOrderStatus == ServiceOrderStatus.Pending ? 1 :
                       order.ServiceOrderStatus == ServiceOrderStatus.Paid ? 2 :
                       order.ServiceOrderStatus == ServiceOrderStatus.Complained ? 3 :
                       order.ServiceOrderStatus == ServiceOrderStatus.TaskFinished ? 4 :
                       order.ServiceOrderStatus == ServiceOrderStatus.DoneTaskComplaint ? 5 :
                       order.ServiceOrderStatus == ServiceOrderStatus.Processing ? 6 :
                       order.ServiceOrderStatus == ServiceOrderStatus.ProcessingComplaint ? 7 :
                       order.ServiceOrderStatus == ServiceOrderStatus.ComplaintCanceled ? 8 :
                       order.ServiceOrderStatus == ServiceOrderStatus.WaitingForPayment ? 9:
                       order.ServiceOrderStatus == ServiceOrderStatus.Completed ? 10 : 11).ThenByDescending(x => x.CreationDate), includes: includes);
                return serviceOrders;
            }
        }

        public async Task<List<ServiceOrderForGardenerViewModel>> GetWorkingCalendar(int month, int year, Guid id)
        {
            var gardener = await _idUtil.GetGardenerAsync(id);
            if (gardener == null)
            {
                throw new Exception("Không tìm thấy người !");
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
                    if (serviceOrder.ServiceOrderStatus == ServiceOrderStatus.ProcessingComplaint)
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
                throw new Exception("Không tồn tại đơn đặt hàng dịch vụ");
            var serviceOrderForGardenerViewModel = _mapper.Map<ServiceOrderForGardenerViewModel>(serviceOrder);
            if (serviceOrder.CustomerBonsaiId == null)
                serviceOrderForGardenerViewModel.ServiceType = 2;
            else
                serviceOrderForGardenerViewModel.ServiceType = 1;
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
            if (serviceOrder.ServiceOrderStatus != Domain.Enums.ServiceOrderStatus.WaitingForPayment)
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
                var serviceOrder = await _unitOfWork.ServiceOrderRepository.GetAllQueryable()
                    .Where(x => x.Id == contractId)
                    .Include(x => x.CustomerGarden.Customer.ApplicationUser)
                    .FirstOrDefaultAsync();
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
                await _unitOfWork.ServiceOrderTransactionRepository.AddAsync(serviceOrderTransaction);
                //Update Contract Status
                serviceOrder.ServiceOrderStatus = serviceOrderStatus;
                _unitOfWork.ServiceOrderRepository.Update(serviceOrder);
                await _unitOfWork.SaveChangeAsync();
                await _notificationService.SendToStaff("Đơn đặt hàng dịch vụ", $"Đơn hàng dịch vụ của {serviceOrder.CustomerGarden.Customer.ApplicationUser} đã được thanh toán thành công");
            }
            catch (Exception exx)
            {
                throw new Exception($"tạo Transaction lỗi: {exx.Message}");
            }
        }
        public async Task<OverallServiceOrderViewModel> GetServiceOrderById(Guid serviceOrderId, bool isCustomer, Guid userId)
        {
            ServiceOrder? serviceOrders;
            if (isCustomer)
            {
                var customer = await _idUtil.GetCustomerAsync(userId);
                //serviceOrders = await _unitOfWork.ServiceOrderRepository.GetAsync(isTakeAll: true, expression: x => x.CustomerGarden.CustomerId == customer.Id && x.Id == serviceOrderId, includes: includes);
                serviceOrders = await _unitOfWork.ServiceOrderRepository
                    .GetAllQueryable()
                    .Where(x => x.CustomerGarden.CustomerId == customer.Id && x.Id == serviceOrderId)
                    .Include(x => x.Contract)
                    .Include(x => x.Complaints)
                    .Include(x => x.Service.ServiceType)
                    .Include(x => x.ServiceOrderGardener).ThenInclude(query => query.Gardener.ApplicationUser)
                    .Include(x => x.ContractTransactions)
                    .FirstOrDefaultAsync();
                if (serviceOrders == null)
                {
                    throw new Exception("Không tìm thấy đơn đặt hàng dịch vụ");
                }
            }
            else
            {
                serviceOrders = await _unitOfWork.ServiceOrderRepository
                   .GetAllQueryable()
                   .Where(x => x.Id == serviceOrderId)
                   .Include(x => x.Contract)
                   .Include(x => x.Complaints)
                   .Include(x => x.Service.ServiceType)
                   .Include(x => x.ServiceOrderGardener).ThenInclude(query => query.Gardener.ApplicationUser)
                   .Include(x => x.ContractTransactions)
                   .FirstOrDefaultAsync();
                if (serviceOrders == null)
                {
                    throw new Exception("Không tìm thấy đơn đặt hàng dịch vụ");
                }
            }
            OverallServiceOrderViewModel overallServiceOrderViewModel = _mapper.Map<OverallServiceOrderViewModel>(serviceOrders);
            overallServiceOrderViewModel.TaskOfServiceOrders = await GetTasksAsync(serviceOrderId, overallServiceOrderViewModel.CustomerBonsaiId != null ? true : false);
            overallServiceOrderViewModel.GardenersOfServiceOrder = await GetGardenerOfServiceOrder(serviceOrderId);
            foreach (Complaint complaint in overallServiceOrderViewModel.Complaints)
            {
                complaint.ComplaintImages = await GetIMageAsync(complaint.Id);
            }
            if (overallServiceOrderViewModel.CustomerBonsaiId != null && overallServiceOrderViewModel.CustomerBonsaiId.HasValue)
            {
                overallServiceOrderViewModel.CustomerBonsai = await _unitOfWork.CustomerBonsaiRepository.GetByIdAsync(overallServiceOrderViewModel.CustomerBonsaiId.Value);
            }
            else
            {
                overallServiceOrderViewModel.CustomerGarden = await _unitOfWork.CustomerGardenRepository.GetByIdAsync(overallServiceOrderViewModel.CustomerGardenId);
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
                if (bonsaiCareSteps.Items != null)
                {
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
                    if (gardenCareTasks.Items != null)
                    {
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
        public async Task AddContract(Guid contractId, ServiceOrderImageModel serviceOrderImageModel)
        {
            if (serviceOrderImageModel.Contract == null)
            {
                throw new Exception("Không có file!");
            }
            //var serviceOrder = await _unitOfWork.ServiceOrderRepository.GetByIdAsync(contractId);
            var serviceOrder = await _unitOfWork.ServiceOrderRepository
                .GetAllQueryable()
                .Where(x => x.Id == contractId)
                .Include(x => x.CustomerGarden)
                .FirstOrDefaultAsync();
            if (serviceOrder == null)
            {
                throw new Exception("Không tìm thấy hợp đồng!");
            }
            try
            {
                _unitOfWork.BeginTransaction();
                var images = await _unitOfWork.ContractRepository.GetAsync(isTakeAll: true, expression: x => x.ServiceOrderId == contractId && !x.IsDeleted, isDisableTracking: true);
                _unitOfWork.ContractRepository.SoftRemoveRange(images.Items);
                string newImageName = serviceOrder.Id + "_i" + serviceOrderImageModel.Contract;
                string folderName = $"contract/{serviceOrder.Id}/pdf";
                string imageExtension = Path.GetExtension(serviceOrderImageModel.Contract.FileName);
                string[] validImageExtensions = {".pdf"};
                const long maxFileSize = 20 * 1024 * 1024;
                if (Array.IndexOf(validImageExtensions, imageExtension.ToLower()) == -1 || serviceOrderImageModel.Contract.Length > maxFileSize)
                {
                    throw new Exception("Có chứa file không phải pdf hoặc quá dung lượng tối đa(>20MB)!");
                }
                var url = await _fireBaseService.UploadFileToFirebaseStorage(serviceOrderImageModel.Contract, newImageName, folderName);
                if (url == null)
                    throw new Exception("Lỗi khi đăng pdf lên firebase!");

                Contract contractImage = new Contract()
                {
                    ServiceOrderId = contractId,
                    Image = url
                };

                await _unitOfWork.ContractRepository.AddAsync(contractImage);
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
                throw new Exception("Không tìm thấy đơn hàng dịch vụ!");
            }
            if (serviceOrderStatus == ServiceOrderStatus.Completed && (serviceOrder.ServiceOrderStatus != ServiceOrderStatus.TaskFinished || serviceOrder.ServiceOrderStatus != ServiceOrderStatus.DoneTaskComplaint) && serviceOrder.EndDate.Date.AddDays(3) > DateTime.Today.Date)
            {
                throw new Exception("Trạng thái không hợp lệ.");
            }
            serviceOrder.ServiceOrderStatus = serviceOrderStatus;
            _unitOfWork.ServiceOrderRepository.Update(serviceOrder);
            await _unitOfWork.SaveChangeAsync();
        }

        public async Task CancelOverdueServiceOrder()
        {
            var listOrder = await _unitOfWork.ServiceOrderRepository.GetAllQueryable().AsNoTracking().Where(x => x.ServiceOrderStatus == ServiceOrderStatus.WaitingForPayment && x.StartDate.Date == DateTime.Now.Date).ToListAsync();
            if(listOrder ==null || listOrder.Count ==0) { }
            foreach (var item in listOrder)
            {
                item.ServiceOrderStatus = ServiceOrderStatus.Fail;
            }
            _unitOfWork.ServiceOrderRepository.UpdateRange(listOrder);
            await _unitOfWork.SaveChangeAsync();
        }
    }
}
