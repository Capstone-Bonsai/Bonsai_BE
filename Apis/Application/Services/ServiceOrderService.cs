using Application.Commons;
using Application.Interfaces;
using Application.Services.Momo;
using Application.Validations.ServiceOrder;
using Application.ViewModels.ServiceOrderViewModels;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Firebase.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Configuration;
using System.Linq.Expressions;
namespace Application.Services
{
    public class ServiceOrderService : IServiceOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly FirebaseService _fireBaseService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public ServiceOrderService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            FirebaseService fireBaseService,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _fireBaseService = fireBaseService;
            _userManager = userManager;
            _configuration = configuration;
        }
        public async Task<Pagination<ServiceOrder>> GetServiceOrdersPagination(int pageIndex, int pageSize)
        {
            var serviceOrders = await _unitOfWork.ServiceOrderRepository.GetAsync(pageIndex: pageIndex, pageSize: pageSize, expression: x => !x.IsDeleted, isDisableTracking: true);
            return serviceOrders;
        }
        public async Task<Pagination<ServiceOrder>> GetServiceOrders()
        {
            var serviceOrders = await _unitOfWork.ServiceOrderRepository.GetAsync(isTakeAll: true, expression: x => !x.IsDeleted, isDisableTracking: true);
            return serviceOrders;
        }
        public async Task AddServiceOrder(ServiceOrderModel serviceOrderModel, Guid customerId)
        {
            bool operationSuccessful = false;

            if (serviceOrderModel == null)
                throw new ArgumentNullException(nameof(serviceOrderModel), "Vui lòng nhập thêm thông tin phân loại!");
            if (serviceOrderModel.TaskId == null || serviceOrderModel.TaskId.Count == 0)
                throw new Exception("Không có công việc nào");
            var validationRules = new ServiceOrderModelValidator();
            var resultServiceOrderInfo = await validationRules.ValidateAsync(serviceOrderModel);
            if (!resultServiceOrderInfo.IsValid)
            {
                var errors = resultServiceOrderInfo.Errors.Select(x => x.ErrorMessage);
                string errorMessage = string.Join(Environment.NewLine, errors);
                throw new Exception(errorMessage);
            }
            var service = await _unitOfWork.ServiceRepository.GetByIdAsync(serviceOrderModel.ServiceId);
            if (service == null)
                throw new Exception("Không tìm thấy dịch vụ yêu cầu!");
            var customer = await GetCustomerAsync(customerId.ToString().ToLower());
            var serviceOrder = new ServiceOrder()
            {
                Id = new Guid(),
                CustomerId = customer.Id,
                CreationDate = DateTime.Now,
                ServiceId = service.Id,
                ServiceType = service.ServiceType,
                Address = serviceOrderModel.Address,
                StandardSquare = service.StandardSqure,
                StandardPrice = service.StandardPrice,
                DiscountPercent = service.DiscountPercent,
                ImplementationTime = service.ImplementationTime,
                StartDate = serviceOrderModel.StartDate,
                EndDate = serviceOrderModel.EndDate,
                GardenSquare = serviceOrderModel.GardenSquare,
                ExpectedWorkingUnit = serviceOrderModel.GardenSquare / service.StandardSqure,
                TemporaryPrice = (serviceOrderModel.GardenSquare / service.StandardSqure) * service.StandardPrice,
                TemporaryTotalPrice = ((serviceOrderModel.GardenSquare / service.StandardSqure) * service.StandardPrice) *
                          (100 - (service.DiscountPercent ?? 0)) / 100,
                ServiceStatus = ServiceStatus.Waiting
            };
            try
            {
                _unitOfWork.BeginTransaction();
                await _unitOfWork.ServiceOrderRepository.AddAsync(serviceOrder);
                if (serviceOrderModel.Image != null)
                {
                    foreach (var singleImage in serviceOrderModel.Image.Select((image, index) => (image, index)))
                    {
                        string newImageName = serviceOrder.Id + "_i" + singleImage.index;
                        string folderName = $"serviceOrder/{serviceOrder.Id}/Image";
                        string imageExtension = Path.GetExtension(singleImage.image.FileName);
                        //Kiểm tra xem có phải là file ảnh không.
                        string[] validImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
                        const long maxFileSize = 20 * 1024 * 1024;
                        if (Array.IndexOf(validImageExtensions, imageExtension.ToLower()) == -1 || singleImage.image.Length > maxFileSize)
                        {
                            throw new Exception("Có chứa file không phải ảnh hoặc quá dung lượng tối đa(>20MB)!");
                        }
                        var url = await _fireBaseService.UploadFileToFirebaseStorage(singleImage.image, newImageName, folderName);
                        if (url == null)
                            throw new Exception("Lỗi khi đăng ảnh lên firebase!");

                        ServiceImage serviceImage = new ServiceImage()
                        {
                            ServiceOrderId = serviceOrder.Id,
                            ImageUrl = url
                        };

                        await _unitOfWork.ServiceImageRepository.AddAsync(serviceImage);
                    }
                    List<DayInWeek> dayInWeeks = new List<DayInWeek>();
                    foreach (ServiceDay date in serviceOrderModel.ServiceDays)
                    {
                        /*var dayInWeek = _unitOfWork.DayInWeekRepository.GetAsync(isTakeAll: true, expression: x => x.ServiceDays == date);
                        dayInWeeks.Add(new OrderServiceTask()
                        {
                            ServiceOrderId = serviceOrder.Id,
                            TaskId = id,
                            ServiceTaskStatus = ServiceTaskStatus.NotYet,
                            Note = ""
                        }); ;*/
                    }
                    List<OrderServiceTask> serviceTasks = new List<OrderServiceTask>();
                    foreach (Guid id in serviceOrderModel.TaskId)
                    {
                        serviceTasks.Add(new OrderServiceTask()
                        {
                            ServiceOrderId = serviceOrder.Id,
                            TaskId = id,
                            ServiceTaskStatus = ServiceTaskStatus.NotYet,
                            Note = ""
                        }); ;
                    }
                    await _unitOfWork.OrderServiceTaskRepository.AddRangeAsync(serviceTasks);
                    await _unitOfWork.CommitTransactionAsync();
                }
            }
            catch (Exception)
            {
                _unitOfWork.RollbackTransaction();
                if (operationSuccessful)
                {
                    foreach (var singleImage in serviceOrderModel.Image.Select((image, index) => (image, index)))
                    {
                        string newImageName = serviceOrder.Id + "_i" + singleImage.index;
                        string folderName = $"serviceOrder/{serviceOrder.Id}/Image";
                        await _fireBaseService.DeleteFileInFirebaseStorage(newImageName, folderName);
                    }
                }
                throw;
            }
        }
        private async Task<Customer> GetCustomerAsync(string userId)
        {
            ApplicationUser? user = null;
            /*if (userId == null || userId.Equals("00000000-0000-0000-0000-000000000000"){ }*/
            user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new Exception("Đã xảy ra lỗi trong quá trình đặt hàng!");
            var isCustomer = await _userManager.IsInRoleAsync(user, "Customer");
            if (!isCustomer)
                throw new Exception("Bạn không có quyền để thực hiện hành động này!");
            var customer = await _unitOfWork.CustomerRepository.GetAllQueryable().FirstOrDefaultAsync(x => x.UserId.ToLower().Equals(user.Id.ToLower()));
            if (customer == null)
                throw new Exception("Không tìm thấy thông tin người dùng");
            return customer;
        }
        public async Task ResponseServiceOrder(Guid staffId, Guid serviceOrderId, ResponseServiceOrderModel responseServiceOrderModel)
        {
            if (responseServiceOrderModel == null)
                throw new ArgumentNullException(nameof(responseServiceOrderModel), "Vui lòng nhập thêm thông tin phân loại!");
            var validationRules = new ResponseServiceOrderModelValidator();
            var resultServiceOrderInfo = await validationRules.ValidateAsync(responseServiceOrderModel);
            if (!resultServiceOrderInfo.IsValid)
            {
                var errors = resultServiceOrderInfo.Errors.Select(x => x.ErrorMessage);
                string errorMessage = string.Join(Environment.NewLine, errors);
                throw new Exception(errorMessage);
            }
            var serviceOrder = await _unitOfWork.ServiceOrderRepository.GetByIdAsync(serviceOrderId);
            if (serviceOrder == null)
                throw new Exception("Không tìm thấy đơn đặt dịch vụ");
            Order? order = null;
            if (responseServiceOrderModel.OrderId != null)
            {
                order = await _unitOfWork.OrderRepository.GetByIdAsync(responseServiceOrderModel.OrderId.Value);
            }
            try
            {
                if (order != null)
                {
                    serviceOrder.OrderId = order.Id;
                    serviceOrder.OrderPrice = order.Price;
                }
                serviceOrder.ResponseGardenSquare = responseServiceOrderModel.ResponseGardenSquare;
                serviceOrder.ResponseStandardSquare = responseServiceOrderModel.ResponseStandardSquare;
                serviceOrder.ResponseWorkingUnit = (int)(Math.Ceiling((decimal)(serviceOrder.ResponseGardenSquare / serviceOrder.ResponseStandardSquare)));
                if (serviceOrder.ServiceType == ServiceType.OneTime)
                {
                    TimeSpan difference = serviceOrder.StartDate - serviceOrder.StartDate;
                    int numberOfDays = (int)difference.TotalDays;
                    serviceOrder.NumberGardener = (serviceOrder.ResponseWorkingUnit / numberOfDays).Value;
                }
                else
                {
                    var serviceDay = await _unitOfWork.ServiceDayRepository.GetAsync(isTakeAll: true, expression: x => x.ServiceOrderId == serviceOrder.Id && !x.IsDeleted);
                    if (serviceDay == null || serviceDay.TotalItemsCount == 0)
                        throw new Exception("Chưa có ngày làm việc");
                    serviceOrder.NumberGardener = (serviceOrder.ResponseWorkingUnit / serviceDay.Items.Count).Value;

                }
                serviceOrder.ResponsePrice = serviceOrder.ResponseWorkingUnit * serviceOrder.StandardPrice;
                serviceOrder.ResponseTotalPrice = serviceOrder.ResponsePrice + serviceOrder.OrderPrice;
                serviceOrder.ResponseFinalPrice = serviceOrder.ResponseTotalPrice *
                          (100 - (serviceOrder.DiscountPercent ?? 0)) / 100;
                serviceOrder.ServiceStatus = ServiceStatus.Applied;
                var staff = await _unitOfWork.GardenerRepository.GetAllQueryable().FirstOrDefaultAsync(x => x.UserId.ToLower().Equals(staffId));
                if (staff == null)
                {
                    throw new Exception("Vui lòng đăng nhập lại!");
                }
                serviceOrder.StaffId = staff.Id;
                _unitOfWork.ServiceOrderRepository.Update(serviceOrder);
                await _unitOfWork.SaveChangeAsync();
            }
            catch (Exception)
            {
                throw new Exception("Đã xảy ra lỗi trong quá trình cập nhật. Vui lòng thử lại!");
            }
        }
        public async Task<ServiceOrder?> GetOrderServiceById(Guid id)
        {
            Pagination<ServiceOrder> orderService;
            List<Expression<Func<ServiceOrder, object>>> includes = new List<Expression<Func<ServiceOrder, object>>>{
                                 x => x.Service,
                                 x => x.Customer.ApplicationUser,
                                 x => x.ServiceImages
                                    };
            orderService = await _unitOfWork.ServiceOrderRepository.GetAsync(isTakeAll: true, expression: x => !x.IsDeleted & x.Id == id,
                isDisableTracking: true, includes: includes);
            return orderService.Items[0];
        }

        public async Task<string> PaymentAsync(Guid tempId)
        {
            var serviceOrder = await _unitOfWork.ServiceOrderRepository.GetByIdAsync(tempId);
            if (serviceOrder == null)
                throw new Exception("Không tìm thấy dịch vụ bạn muốn thanh toán!");
            else if (serviceOrder.ServiceStatus != ServiceStatus.Applied || serviceOrder.ServiceStatus != ServiceStatus.Failed)
                throw new Exception("Yêu cầu dịch vụ này chưa đủ điều kiện để tiến hành thanh toán!");
            else if (serviceOrder.ResponseFinalPrice == null)
                throw new Exception("Yêu cầu dịch vụ này chưa đủ điều kiện để tiến hành thanh toán!");
            double totalPrice = Math.Round(serviceOrder.ResponseFinalPrice.Value);
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
                throw new Exception("Đã xảy ra lối trong qua trình thanh toán. Vui lòng thanh toán lại sau!");
            }
        }

        /*public async Task HandleIpnAsync(MomoRedirect momo)
        {
            string accessKey = _configuration["MomoServices:accessKey"];
            string IpnUrl = _configuration["MomoServices:ipnUrl"];
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
            OrderStatus orderStatus = OrderStatus.Failed;
            //check chữ ký
            if (temp != momo.signature)
                throw new Exception("Sai chữ ký");
            //lấy orderid
            Guid orderId = Guid.Parse(momo.extraData);
            try
            {
                if (momo.resultCode == 0)
                {
                    transactionStatus = TransactionStatus.Success;
                    orderStatus = OrderStatus.Paid;
                }
                var order = await _unit.OrderRepository.GetByIdAsync(orderId);
                if (order == null)
                    throw new Exception("Không tìm thấy đơn hàng.");
                var orderTransaction = new OrderTransaction();
                orderTransaction.OrderId = orderId;
                orderTransaction.Amount = momo.amount;
                orderTransaction.IpnURL = IpnUrl;
                orderTransaction.Information = momo.orderInfo;
                orderTransaction.PartnerCode = partnerCode;
                orderTransaction.RedirectUrl = redirectUrl;
                orderTransaction.RequestId = momo.requestId;
                orderTransaction.RequestType = "captureWallet";
                orderTransaction.TransactionStatus = transactionStatus;
                orderTransaction.PaymentMethod = "MOMO Payment";
                orderTransaction.OrderIdFormMomo = momo.orderId;
                orderTransaction.OrderType = momo.orderType;
                orderTransaction.TransId = momo.transId;
                orderTransaction.ResultCode = momo.resultCode;
                orderTransaction.Message = momo.message;
                orderTransaction.PayType = momo.payType;
                orderTransaction.ResponseTime = momo.responseTime;
                orderTransaction.ExtraData = momo.extraData;
                // Tạo transaction
                orderTransaction.Signature = momo.signature;
                await _unit.OrderTransactionRepository.AddAsync(orderTransaction);
                //Update Order Status
                order.OrderStatus = orderStatus;
                _unit.OrderRepository.Update(order);
                await _unit.SaveChangeAsync();
                if (momo.resultCode != 0)
                {
                    await UpdateProductQuantityFromOrder(orderId);
                }
            }
            catch (Exception exx)
            {
                throw new Exception($"tạo Transaction lỗi: {exx.Message}");
            }
        }*/
    }
}
