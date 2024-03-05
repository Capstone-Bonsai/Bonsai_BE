using Application.Commons;
using Application.Interfaces;
using Application.Repositories;
using Application.Services.Momo;
using Application.Validations.Order;
using Application.ViewModels.DeliveryFeeViewModels;
using Application.ViewModels.OrderDetailModels;
using Application.ViewModels.OrderViewModels;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Linq.Expressions;

namespace Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unit;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IDeliveryFeeService _deliveryFeeService;

        public OrderService( IConfiguration configuration, IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager,IMapper mapper, IDeliveryFeeService deliveryFeeService)
        {
            _configuration = configuration;
            _unit = unitOfWork;
            _userManager = userManager;
            _mapper = mapper;
            _deliveryFeeService = deliveryFeeService;
        }
        public async Task<IList<string>> ValidateOrderModel(OrderModel model, string userId)
        {
            if (model == null)
            {
                throw new Exception("Vui lòng thêm các thông tin mua hàng.");
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

            if (model.ListProduct == null || model.ListProduct.Count == 0)
            {
                throw new Exception("Vui lòng chọn sản phẩm bạn muốn mua.");
            }
            var orderDetailValidate = new OrderDetailModelValidator();
            foreach (var item in model.ListProduct)
            {
                var resultOrderDetail = await orderDetailValidate.ValidateAsync(item);
                if (!resultOrderDetail.IsValid)
                {
                    var errors = new List<string>();
                    errors.AddRange(resultOrderDetail.Errors.Select(x => x.ErrorMessage));
                    return errors;
                }

            }
            return null;
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
                throw new Exception("Đã xảy ra lối trong qua trình thanh toán. Vui lòng thanh toán lại sau!");

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
        }

        public async Task UpdateProductQuantityFromOrder(Guid orderId)
        {
            var order = await _unit.OrderRepository.GetAllQueryable().Include(x => x.OrderDetails).Where(x => !x.IsDeleted && x.Id == orderId && x.OrderStatus == OrderStatus.Failed).FirstOrDefaultAsync();
            if (order == null)
                throw new Exception("Không tìm thấy đơn hàng.");
            foreach (var item in order.OrderDetails)
            {
                var product = await _unit.ProductRepository.GetByIdAsync(item.ProductId);
                if (product == null)
                    throw new Exception("Không tìm thấy sản phẩm bạn muốn mua");

                //cộng quantity của product
                product.Quantity += item.Quantity;
                _unit.ProductRepository.Update(product);
                await _unit.SaveChangeAsync();
            }
        }

        public async Task<Pagination<OrderViewModel>> GetPaginationAsync(string userId, int pageIndex = 0, int pageSize = 10)
        {
            Pagination<Order> res;
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new Exception("Không tìm thấy người dùng!");
            var isCustomer = await _userManager.IsInRoleAsync(user, "Customer");
            var isAdmin = await _userManager.IsInRoleAsync(user, "Manager");
            var isStaff = await _userManager.IsInRoleAsync(user, "Staff");
            List<Expression<Func<Order, object>>> includes = new List<Expression<Func<Order, object>>>
{
    x => x.Customer.ApplicationUser
};
            if (isCustomer && !isAdmin && !isStaff)
                res = await _unit.OrderRepository.GetAsync(expression: x => x.Customer.UserId.ToLower() == userId, isDisableTracking: true, includes: includes, pageIndex: pageIndex, pageSize: pageSize, orderBy: x => x.OrderByDescending(y => y.CreationDate));
            else if (isAdmin || isStaff)
                res = await _unit.OrderRepository.GetAsync(isDisableTracking: true, includes: includes, pageIndex: pageIndex, pageSize: pageSize, orderBy: x => x.OrderByDescending(y => y.CreationDate));
            else return null;
            var result = _mapper.Map<Pagination<OrderViewModel>>(res);
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
            /*List<Expression<Func<Order, object>>> includes = new List<Expression<Func<Order, object>>>
{
    x => x.Customer.ApplicationUser,
    x=>x.OrderTransaction,
    x=>x.OrderDetails.Select( y =>y.Product).Where(i=>!i.IsDeleted),
};

            var orders = await _unit.OrderRepository.GetAsync(isDisableTracking: true, includes: includes, isTakeAll: true, expression: x=>x.Id == orderId);*/
            var order = await _unit.OrderRepository.GetAllQueryable().AsNoTracking().
                Include(x=>x.OrderTransaction).Include(x=>x.Customer.ApplicationUser).Include(x=>x.OrderDetails.Where(i=>!i.IsDeleted)).ThenInclude(x=>x.Product.ProductImages).
                FirstOrDefaultAsync(x=>x.Id == orderId);
            if (order ==null )
                throw new Exception("Không tìm thấy đơn hàng bạn yêu cầu");
            
            if (isCustomer && !order.Customer.UserId.ToLower().Equals(userId.ToLower()))
                throw new Exception("Bạn không có quyền truy cập vào đơn hàng này!");
            return order;
        }
        public async Task<Guid> CreateOrderByTransaction(OrderModel model, string? userId)
        {
            var customer = await GetCustomerAsync(model, userId);
            _unit.BeginTransaction();
            try
            {
                Guid orderId = await CreateOrder(model, customer.Id);
                foreach (var item in model.ListProduct)
                {
                    await CreateOrderDetail(item, orderId);
                }
                await UpdateOrder(orderId);
                await _unit.CommitTransactionAsync();
                return orderId;
            }
            catch (Exception ex)
            {
                _unit.RollbackTransaction();
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
            if (userId == null || userId.Equals("00000000-0000-0000-0000-000000000000"))
            {
                if (model.OrderInfo == null)
                    throw new Exception("Vui lòng thêm các thông tin người mua hàng.");
                user = await _userManager.FindByEmailAsync(model.OrderInfo.Email);
                if (user == null)
                {
                    var result = await CreateUserAsync(model.OrderInfo);
                    if (!result.Succeeded)
                    {
                        throw new Exception("Đã xảy ra lỗi trong quá trình đặt hàng!");
                    }
                    else
                    {
                        user = await _userManager.FindByEmailAsync(model.OrderInfo.Email);
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
            }
            else
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

        public async Task<Guid> CreateOrder(OrderModel model, Guid customerId)
        {
            try
            {
                var order = _mapper.Map<Order>(model);
                order.DeliveryType = Domain.Enums.DeliveryType.PickupTruck;
                order.OrderDate = DateTime.Now;
                order.CustomerId = customerId;
                order.Price = 0;
                order.DeliveryPrice = 0;
                order.TotalPrice = 0;
                order.OrderStatus = Domain.Enums.OrderStatus.Waiting;
                order.OrderType = Domain.Enums.OrderType.Nomial;
                await _unit.OrderRepository.AddAsync(order);
                await _unit.SaveChangeAsync();
                return order.Id;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task CreateOrderDetail(OrderDetailModel model, Guid orderId)
        {
            try
            {
                var product = await _unit.ProductRepository.GetByIdAsync(model.ProductId);
                if (product == null)
                    throw new Exception("Không tìm thấy sản phẩm bạn muốn mua");
                else if ((product.Quantity - model.Quantity) < 0)
                    throw new Exception($"Số lượng sản phẩm {product.Name} trong kho không đủ số lượng bạn yêu cầu.");
                //tạo order đetail
                var orderDetail = _mapper.Map<OrderDetail>(model);
                orderDetail.OrderId = orderId;
                orderDetail.UnitPrice = product.UnitPrice;
                await _unit.OrderDetailRepository.AddAsync(orderDetail);

                //trừ quantity của product
                product.Quantity -= model.Quantity;
                _unit.ProductRepository.Update(product);
                await _unit.SaveChangeAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public async Task UpdateOrder(Guid orderId)
        {
            try
            {
                var order = await _unit.OrderRepository.GetAllQueryable().AsNoTracking().Where(x => x.Id == orderId && !x.IsDeleted).FirstOrDefaultAsync();
                if (order == null)
                    throw new Exception("Đã xảy ra lỗi trong quá trình đặt hàng!");
                List<Expression<Func<OrderDetail, object>>> includes = new List<Expression<Func<OrderDetail, object>>>{
                                 x => x.Product
                                    };
                var listOrderDetail = await _unit.OrderDetailRepository.GetAsync(expression: x => x.OrderId == orderId && !x.IsDeleted, isDisableTracking: true, isTakeAll: true, includes: includes);

                if (listOrderDetail == null || listOrderDetail.TotalItemsCount == 0)
                    throw new Exception("Đã xảy ra lỗi trong quá trình đặt hàng!");
                double total = 0;
                foreach (var item in listOrderDetail.Items)
                {
                    var temp = item.Quantity * item.Product.UnitPrice;
                    total += temp;
                }
                FeeViewModel deliveryPrice = new FeeViewModel();
                deliveryPrice = await CalculateDeliveryPrice(order.Address, total);
                order.DeliveryType = deliveryPrice.deliveryFee.DeliveryType;
                order.DeliveryPrice = deliveryPrice.Price;
                order.Price = total;
                order.TotalPrice = total + deliveryPrice.Price;
                _unit.ClearTrack();
                _unit.OrderRepository.Update(order);
                await _unit.SaveChangeAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public async Task<FeeViewModel> CalculateDeliveryPrice(string destination, double price)
        {
            var distance = await _deliveryFeeService.CalculateFee(destination, price);
            return distance;
        }

        public async Task UpdateOrderStatusAsync(Guid orderId, OrderStatus orderStatus)
        {
            var order = await _unit.OrderRepository.GetByIdAsync(orderId);
            if(order == null)
            {
                throw new Exception("Không tìm thấy đơn hàng bạn yêu cầu.");
            }
            if(orderStatus < order.OrderStatus)
            {
                throw new Exception("Trạng thái không hợp lệ.");
            }
            order.OrderStatus = orderStatus;
            _unit.OrderRepository.Update(order);
            await _unit.SaveChangeAsync();
        }
    }
}


