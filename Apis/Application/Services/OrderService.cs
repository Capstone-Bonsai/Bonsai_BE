using Application.Commons;
using Application.Interfaces;
using Application.Repositories;
using Application.Services.Momo;
using Application.Utils;
using Application.Validations.Auth;
using Application.Validations.Order;
using Application.ViewModels.AuthViewModel;
using Application.ViewModels.BonsaiViewModel;
using Application.ViewModels.DeliveryFeeViewModels;
using Application.ViewModels.OrderViewModels;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Firebase.Auth;
using MailKit.Search;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Linq.Expressions;
using System.Text.Encodings.Web;

namespace Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unit;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IDeliveryFeeService _deliveryFeeService;
        private readonly IFirebaseService _fireBaseService;
        private readonly IdUtil _idUtil;
        private readonly INotificationService _notificationService;
        public OrderService(IConfiguration configuration, IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, IMapper mapper, IDeliveryFeeService deliveryFeeService, FirebaseService fireBaseService, IdUtil idUtil, INotificationService notificationService)
        {
            _configuration = configuration;
            _unit = unitOfWork;
            _userManager = userManager;
            _mapper = mapper;
            _deliveryFeeService = deliveryFeeService;
            _fireBaseService = fireBaseService;
            _idUtil = idUtil;
            _notificationService = notificationService;
        }
        public async Task<IList<string>> ValidateOrderModel(OrderModel model, string userId)
        {
            if (model == null)
            {
                throw new Exception("Vui lòng điền đầu đủ thông tin.");
            }
            else if (userId == null && model.OrderInfo == null)
            {
                throw new Exception("Vui lòng thêm các thông tin người mua hàng.");
            }
            else if (model.OrderInfo != null)
            {
                var orderInfoValidate = new OrderInfoModelValidator();
                var resultOrderInfo = await orderInfoValidate.ValidateAsync(model.OrderInfo);
                if (!resultOrderInfo.IsValid)
                {
                    var errors = new List<string>();
                    errors.AddRange(resultOrderInfo.Errors.Select(x => x.ErrorMessage));
                    return errors;
                }
            }
            var validator = new OrderModelValidator();
            var result = await validator.ValidateAsync(model);
            if (!result.IsValid)
            {
                var errors = new List<string>();
                errors.AddRange(result.Errors.Select(x => x.ErrorMessage));
                return errors;
            }

            if (model.ListBonsai == null || model.ListBonsai.Count == 0)
            {
                throw new Exception("Vui lòng chọn bonsai bạn muốn mua.");
            }
            return null;
        }

        public async Task GenerateTokenAsync(OrderInfoModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                var result = await CreateUserAsync(model);
                if (!result.Succeeded)
                {
                    throw new Exception("Đã xảy ra lỗi trong quá trình đặt hàng!");
                }
                else
                {
                    user = await _userManager.FindByEmailAsync(model.Email);
                    //tạo account customer.
                    try
                    {
                        await _userManager.AddToRoleAsync(user, "Customer");
                        Customer cus = new Customer { UserId = user.Id };
                        await _unit.CustomerRepository.AddAsync(cus);
                        await _unit.SaveChangeAsync();
                    }
                    catch (Exception)
                    {
                        await _userManager.DeleteAsync(user);
                        throw new Exception("Đã xảy ra lỗi trong quá trình đăng ký. Vui lòng thử lại!");
                    }
                }
            }
            else
            {
                user = await _userManager.FindByEmailAsync(model.Email);
                if (user.PhoneNumber != model.PhoneNumber && user.IsRegister == true)
                {
                    throw new Exception("Vui lòng nhập đúng số điện thoại của tài khoản đã đăng ký!");
                }
                else if (user.PhoneNumber != model.PhoneNumber && user.IsRegister == false)
                {
                    throw new Exception($"Vui lòng nhập đúng số điện thoại của đã đặt hàng ({FormatPhoneNumber(user.PhoneNumber.ToString())})!");
                }
            }
            var isCustomer = await _userManager.IsInRoleAsync(user, "Customer");
            if (!isCustomer)
                throw new Exception("Bạn không có quyền để thực hiện hành động này!");
            var customer = await _unit.CustomerRepository.GetAllQueryable().FirstOrDefaultAsync(x => x.UserId.ToLower().Equals(user.Id.ToLower()));
            if (customer == null)
                throw new Exception("Không tìm thấy thông tin người dùng");

            var token = await _userManager.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultPhoneProvider);
            await SendEmailAsync(model.Email.Trim(), token);
        }
        public async Task<bool> SendEmailAsync(string email, string callbackUrl)
        {

            MailService mail = new MailService();
            var temp = mail.SendEmail(email, "Xác nhận email từ Thanh Sơn Garden",
            $"<h2 style=\" color: #00B214;\">Xác thực email từ Thanh Sơn Garden</h2>\r\n<p style=\"margin-bottom: 10px;\r\n    text-align: left;\">Xin chào <strong>{email}</strong>"
            + ",</p>\r\n<p style=\"margin-bottom: 10px;\r\n    text-align: left;\"> Cảm ơn bạn đã quan tâm tới Thanh Sơn Garden." +
            " Để có được trải nghiệm dịch vụ và được hỗ trợ tốt nhất, bạn cần xác thực địa chỉ email.</p>"+ $"Mã xác thực: {callbackUrl}" );
            var result = (temp) ? true : false;
            return result;

        }

        public async Task<string> CreateOrderAsync(OrderModel model, string userId)
        {
            var orderId = await CreateOrderByTransaction(model, userId);
            var momoUrl = await PaymentAsync(orderId);
            return momoUrl;
        }
        public async Task<string> PaymentAsync(Guid tempId)
        {
            var order = await _unit.OrderRepository.GetByIdAsync(tempId);
            if (order == null)
                throw new Exception("Đã xảy ra lỗi trong quá trình thanh toán. Vui lòng thanh toán lại sau");

            double totalPrice = Math.Round(order.TotalPrice);
            string endpoint = _configuration["MomoServices:endpoint"];
            string partnerCode = _configuration["MomoServices:partnerCode"];
            string accessKey = _configuration["MomoServices:accessKey"];
            string serectkey = _configuration["MomoServices:secretKey"];
            string orderInfo = "Thanh toán hóa đơn hàng tại Thanh Sơn Garden.";
            string redirectUrl = _configuration["MomoServices:redirectUrl"];
            string ipnUrl = _configuration["MomoServices:ipnUrl"];
            string requestType = "captureWallet";
            string amount = totalPrice.ToString();
            string orderId = Guid.NewGuid().ToString();
            string requestId = Guid.NewGuid().ToString();
            string extraData = order.Id.ToString();
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

        public async Task HandleIpnAsync(MomoRedirect momo)
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
                var order = await _unit.OrderRepository.GetAllQueryable().AsNoTracking().FirstOrDefaultAsync(x => x.Id == orderId);
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

                if (momo.resultCode != 0)
                {
                    var lists = new List<Bonsai>();
                    var listBonsai = await _unit.OrderDetailRepository.GetAllQueryable().Include(x => x.Bonsai).Where(x => x.IsDeleted == false && x.OrderId == orderId).ToListAsync();
                    foreach (var item in listBonsai)
                    {
                        item.Bonsai.isSold = false;
                        item.Bonsai.isDisable = false;
                        lists.Add(item.Bonsai);
                    }
                    _unit.ClearTrack();
                    _unit.BonsaiRepository.UpdateRange(lists);
                    await _unit.SaveChangeAsync();
                }
                //Update Order Status
                order.OrderStatus = orderStatus;
                _unit.OrderRepository.Update(order);
                await _unit.SaveChangeAsync();
                if (momo.resultCode != 0)
                {
                    await UpdateBonsaiFromOrder(orderId);
                }
            }
            catch (Exception exx)
            {
                throw new Exception($"tạo Transaction lỗi: {exx.Message}");
            }
        }

        public async Task UpdateBonsaiFromOrder(Guid orderId)
        {
            var order = await _unit.OrderRepository.GetAllQueryable().Include(x => x.OrderDetails).Where(x => !x.IsDeleted && x.Id == orderId && x.OrderStatus == OrderStatus.Failed).FirstOrDefaultAsync();
            if (order == null)
                throw new Exception("Không tìm thấy đơn hàng.");
            foreach (var item in order.OrderDetails)
            {
                var bonsai = await _unit.BonsaiRepository.GetByIdAsync(item.BonsaiId);
                if (bonsai == null)
                    throw new Exception("Không tìm thấy bonsai bạn muốn mua");
                bonsai.isSold = false;
                bonsai.isDisable = false;
                _unit.BonsaiRepository.Update(bonsai);
                await _unit.SaveChangeAsync();
            }
        }

        public async Task<Pagination<OrderViewModel>> GetPaginationAsync(string userId, int pageIndex = 0, int pageSize = 10)
        {
            IList<Order> listOrder = new List<Order>();
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new Exception("Không tìm thấy người dùng!");
            var isCustomer = await _userManager.IsInRoleAsync(user, "Customer");
            var isAdmin = await _userManager.IsInRoleAsync(user, "Manager");
            var isStaff = await _userManager.IsInRoleAsync(user, "Staff");
            var isGardener = await _userManager.IsInRoleAsync(user, "Gardener");
            if (isCustomer && !isAdmin && !isStaff)
                listOrder = await _unit.OrderRepository.GetAllQueryable().AsNoTracking()
                    .Include(x => x.Customer)
                .Include(x => x.OrderDetails)
                .ThenInclude(x => x.Bonsai.BonsaiImages)
                .Include(x => x.DeliveryImages)
                .Where(x => x.Customer.UserId.ToLower() == userId).OrderByDescending(y => y.CreationDate).ToListAsync();
            else if (isAdmin || isStaff)
                listOrder = await _unit.OrderRepository.GetAllQueryable().AsNoTracking()
                   .Include(x => x.Customer.ApplicationUser)
               .Include(x => x.OrderDetails)
               .ThenInclude(x => x.Bonsai.BonsaiImages).Include(x=>x.OrderTransaction)
               .Include(x => x.DeliveryImages)

               .OrderByDescending(y => y.CreationDate).ToListAsync();
            else if (isGardener)
            {
                var gardener = await _idUtil.GetGardenerAsync(Guid.Parse(userId));
                listOrder = await _unit.OrderRepository.GetAllQueryable().AsNoTracking()
                   .Include(x => x.Customer.ApplicationUser)
               .Include(x => x.OrderDetails)
               .ThenInclude(x => x.Bonsai.BonsaiImages)
               .Where(x => x.GardenerId == gardener.Id && (x.OrderStatus == OrderStatus.Preparing || x.OrderStatus == OrderStatus.Delivering)).OrderByDescending(y => y.CreationDate).ToListAsync();
            }
            else return null;
            var itemCount = listOrder.Count();
            var items = listOrder.OrderByDescending(x => x.CreationDate)
                                    .Skip(pageIndex * pageSize)
                                    .Take(pageSize)
                                    .ToList();

            var res = new Pagination<Order>()
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalItemsCount = itemCount,
                Items = items,
            };

            var result = _mapper.Map<Pagination<OrderViewModel>>(res);
            foreach(OrderViewModel orderViewModel in result.Items)
            {
                if (orderViewModel.GardenerId != null)
                {
                    orderViewModel.Gardener = await _unit.GardenerRepository.GetAllQueryable()
                   .Where(x => x.Id == orderViewModel.GardenerId)
                   .Include(x => x.ApplicationUser)
                   .FirstOrDefaultAsync();
                }   
            }
            return result;
        }
        public async Task<Order> GetByIdAsync(string userId, Guid orderId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new Exception("Không tìm thấy người dùng!");
            var isCustomer = await _userManager.IsInRoleAsync(user, "Customer");
            var isAdmin = await _userManager.IsInRoleAsync(user, "Manager");
            var isStaff = await _userManager.IsInRoleAsync(user, "Staff");
            var isGardener = await _userManager.IsInRoleAsync(user, "Gardener");
            var order = await _unit.OrderRepository.GetAllQueryable().AsNoTracking().
                Include(x => x.OrderTransaction).Include(x => x.Customer.ApplicationUser).Include(x => x.OrderDetails.Where(i => !i.IsDeleted)).ThenInclude(x => x.Bonsai.BonsaiImages).
                FirstOrDefaultAsync(x => x.Id == orderId);
            if (order == null)
                throw new Exception("Không tìm thấy đơn hàng bạn yêu cầu");

            if (isCustomer && !order.Customer.UserId.ToLower().Equals(userId.ToLower()))
                throw new Exception("Bạn không có quyền truy cập vào đơn hàng này!");
            return order;
        }
        public async Task<Guid> CreateOrderByTransaction(OrderModel model, string? userId)
        {

            var customer = await GetCustomerAsync(model, userId);
            var user = await _userManager.FindByIdAsync(customer.UserId);
            var isValid = await _userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultPhoneProvider, model.OtpCode);
            if (!isValid)
            {
                throw new Exception("Mã OTP không chính xác.");
            }
            await _userManager.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultPhoneProvider);
            _unit.BeginTransactionLocking();
            try
            {
                Guid orderId = await CreateOrder(model, customer.Id);
                foreach (var item in model.ListBonsai)
                {
                    await CreateOrderDetail(item, orderId);
                }
                await UpdateOrder(orderId, model.ListBonsai.Distinct().ToList());
                await _unit.CommitTransactionAsync();
                return orderId;
            }
            catch (Exception ex)
            {
                _unit.RollbackTransaction();
                if (ex.Message.Contains("An exception has been raised that is likely due to a transient failure")) throw new Exception("Đã xảy ra lỗi trong quá trình đặt hàng!");

                throw new Exception(ex.Message);
            }
        }



        public async Task<IdentityResult> CreateUserAsync(OrderInfoModel model)
        {
            var user = new ApplicationUser
            {
                Email = model.Email,
                Fullname = model.Fullname,
                PhoneNumber = model.PhoneNumber,
                UserName = model.Email,
                IsRegister = false,
                TwoFactorEnabled = true
            };

            var result = await _userManager.CreateAsync(user);

            return result;
        }

        public async Task<Customer> GetCustomerAsync(OrderModel model, string? userId)
        {
            ApplicationUser? user = null;
            if (userId == null || userId.Equals("00000000-0000-0000-0000-000000000000")) //iff not login
            {
                if (model.OrderInfo == null)
                    throw new Exception("Vui lòng thêm các thông tin người mua hàng.");
                user = await _userManager.FindByEmailAsync(model.OrderInfo.Email);
                if (user == null)
                {
                    throw new Exception("Vui lòng xác thực email của bạn trước khi đặt hàng");
                }
                else
                {
                    if(user.PhoneNumber != model.OrderInfo.PhoneNumber && user.IsRegister == true){
                        throw new Exception("Vui lòng nhập đúng số điện thoại của tài khoản đã đăng ký!");
                    }
                    else if(user.PhoneNumber != model.OrderInfo.PhoneNumber && user.IsRegister == false)
                    {
                        throw new Exception($"Vui lòng nhập đúng số điện thoại của đã đặt hàng ({FormatPhoneNumber(user.PhoneNumber.ToString())})!");
                    }
                }
            }
            else //if login
            {
                user = await _userManager.FindByIdAsync(userId);
            }
            if (user == null)
                throw new Exception("Đã xảy ra lỗi trong quá trình đặt hàng!");
            var isCustomer = await _userManager.IsInRoleAsync(user, "Customer");
            if (!isCustomer)
                throw new Exception("Bạn không có quyền để thực hiện hành động này!");
            var customer = await _unit.CustomerRepository.GetAllQueryable().FirstOrDefaultAsync(x => x.UserId.ToLower().Equals(user.Id.ToLower()));
            if (customer == null)
                throw new Exception("Không tìm thấy thông tin người dùng");
            return customer;
        }

        private string FormatPhoneNumber(string phoneNumber)
        {
            if (phoneNumber.Length != 10)
            {
                throw new ArgumentException("Số điện thoại không đúng định dang.");
            }

            string firstTwoDigits = phoneNumber.Substring(0, 2);
            string lastThreeDigits = phoneNumber.Substring(7, 3);

            return $"{firstTwoDigits}*****{lastThreeDigits}";
        }


        public async Task<Guid> CreateOrder(OrderModel model, Guid customerId)
        {
            try
            {
                var order = _mapper.Map<Order>(model);
                order.CustomerId = customerId;
                order.Price = 0;
                order.DeliveryPrice = 0;
                order.TotalPrice = 0;
                order.OrderStatus = Domain.Enums.OrderStatus.Waiting;
                await _unit.OrderRepository.AddAsync(order);
                await _unit.SaveChangeAsync();
                return order.Id;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task CreateOrderDetail(Guid bonsaiId, Guid orderId)
        {
            try
            {
                var bonsai = await _unit.BonsaiRepository.GetByIdAsync(bonsaiId);
                if (bonsai == null)
                    throw new Exception("Không tìm thấy bonsai bạn muốn mua");
                else if (bonsai.isSold == null)
                    throw new Exception($"{bonsai.Name} không tồn tại");
                else if (bonsai.isSold == true)
                    throw new Exception($"{bonsai.Name} đã được bán");
                else if (bonsai.isDisable == true)
                    throw new Exception($"{bonsai.Name} không khả dụng");

                //tạo order đetail
                var orderDetail = new OrderDetail();
                orderDetail.BonsaiId = bonsaiId;
                orderDetail.OrderId = orderId;
                orderDetail.Price = bonsai.Price;
                await _unit.OrderDetailRepository.AddAsync(orderDetail);

                //trừ quantity của product
                bonsai.isSold = true;
                bonsai.isDisable = true;
                _unit.BonsaiRepository.Update(bonsai);
                await _unit.SaveChangeAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public async Task UpdateOrder(Guid orderId, List<Guid> bonsaisId)
        {
            try
            {
                var order = await _unit.OrderRepository.GetAllQueryable().AsNoTracking().Where(x => x.Id == orderId && !x.IsDeleted).FirstOrDefaultAsync();
                if (order == null)
                    throw new Exception("Đã xảy ra lỗi trong quá trình đặt hàng!");

                var listOrderDetail = await _unit.OrderDetailRepository.GetAllQueryable().Where(x => x.OrderId == orderId && !x.IsDeleted).AsNoTracking().ToListAsync();

                if (listOrderDetail == null || listOrderDetail.Count == 0)
                    throw new Exception("Đã xảy ra lỗi trong quá trình đặt hàng!");
                double total = 0;
                foreach (var item in listOrderDetail)
                {
                    total += item.Price;
                }
                FeeViewModel deliveryPrice = new FeeViewModel();
                deliveryPrice = await _deliveryFeeService.CalculateFeeOrder(order.Address, bonsaisId);

                order.DeliveryPrice = deliveryPrice.DeliveryFee;
                order.ExpectedDeliveryDate = deliveryPrice.ExpectedDeliveryDate;
                order.Price = total;
                order.TotalPrice = total + deliveryPrice.DeliveryFee;
                _unit.ClearTrack();
                _unit.OrderRepository.Update(order);
                await _unit.SaveChangeAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public async Task<FeeViewModel> CalculateDeliveryPrice(string destination, IList<Guid> listBonsaiId)
        {
            var distance = await _deliveryFeeService.CalculateFee(destination, listBonsaiId);
            return distance;
        }

        public async Task UpdateOrderStatusAsync(Guid orderId, OrderStatus orderStatus)
        {
            var order = await _unit.OrderRepository.GetByIdAsync(orderId);
            if (order == null)
            {
                throw new Exception("Không tìm thấy");
            }
            if (orderStatus < order.OrderStatus)
            {
                throw new Exception("Trạng thái không hợp lệ.");
            }
            if (order.OrderStatus == OrderStatus.Delivered || order.OrderStatus == OrderStatus.DeliveryFailed || order.OrderStatus == OrderStatus.Failed) throw new Exception("Đơn hàng này đã kết thúc nên không thể cập nhật trạng thái.");
            order.OrderStatus = orderStatus;
            _unit.OrderRepository.Update(order);
            await _unit.SaveChangeAsync();
        }
        public async Task FinishDeliveryOrder(Guid orderId, FinishDeliveryOrderModel finishDeliveryOrderModel)
        {
            var order = await _unit.OrderRepository.GetByIdAsync(orderId);
            if (order == null)
            {
                throw new Exception("Không tìm thấy");
            }
            try
            {
                _unit.BeginTransaction();
                foreach (var singleImage in finishDeliveryOrderModel.Image.Select((image, index) => (image, index)))
                {
                    string newImageName = order.Id + "_i" + singleImage.index;
                    string folderName = $"order/{order.Id}/Image";
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

                    DeliveryImage deliveryImage = new DeliveryImage()
                    {
                        OrderId = order.Id,
                        Image = url
                    };
                    await _unit.DeliveryImageRepository.AddAsync(deliveryImage);
                }
                order.OrderStatus = OrderStatus.Delivered;
                order.DeliveryDate = DateTime.Now;

                _unit.OrderRepository.Update(order);
                await _unit.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                _unit.RollbackTransaction();
                throw new Exception(ex.Message);
            }
        }
        public async Task AddGardenerForOrder(Guid orderId, Guid gardenerId)
        {
            var order = await _unit.OrderRepository.GetByIdAsync(orderId);
            if (order == null)
            {
                throw new Exception("Không tìm thấy");
            }
            try
            {
                order.GardenerId = gardenerId;
                order.OrderStatus = OrderStatus.Preparing;
                _unit.OrderRepository.Update(order);
                await _unit.SaveChangeAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task CreateNotificationForStaff(Guid userId, Guid orderId)
        {
            var order = await _unit.OrderRepository
                .GetAllQueryable()
                .Include(x => x.Customer.ApplicationUser)
                .Where(x => x.Id == orderId)
                .FirstOrDefaultAsync();
            if (order == null)
            {
                return;
            }

            if (order.OrderDate == new DateTime(2020, 1, 1) && (order.OrderStatus == OrderStatus.Paid))
            {
                await _notificationService.SendToStaff("Thông báo đơn đặt hàng", $"Đơn đặt hàng {order.Customer.ApplicationUser.Email} đã thanh toán thành công");
                await _notificationService.SendMessageForUserId(Guid.Parse(order.Customer.ApplicationUser.Id), "Thông báo đơn đặt hàng", $"Đơn đặt hàng đã thanh toán thành công");
                order.OrderDate = DateTime.Now;
                _unit.OrderRepository.Update(order);
                await _unit.SaveChangeAsync();
            }
        }
    }
}


