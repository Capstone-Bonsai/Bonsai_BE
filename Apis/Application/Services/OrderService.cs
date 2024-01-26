using Application.Interfaces;
using Application.Repositories;
using Application.Services.Momo;
using Application.Validations.Order;
using Application.ViewModels.OrderViewModels;

using Domain.Entities;
using Domain.Enums;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;


namespace Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;

        public OrderService(ITransactionRepository transactionRepository, IConfiguration configuration, IUnitOfWork unitOfWork)
        {
            _transactionRepository = transactionRepository;
            _configuration = configuration;
            _unitOfWork = unitOfWork;

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
            var orderId = await _transactionRepository.CreateOrderByTransaction(model, userId);
            var momoUrl = await PaymentAsync(orderId);
            return momoUrl;
        }
        public async Task<string> PaymentAsync(Guid tempId)
        {
            var order = await _unitOfWork.OrderRepository.GetByIdAsync(tempId);
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
                var order = await _unitOfWork.OrderRepository.GetByIdAsync(orderId);
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
                await _unitOfWork.OrderTransactionRepository.AddAsync(orderTransaction);
                //Update Order Status
                order.OrderStatus = orderStatus;
                await _unitOfWork.SaveChangeAsync();
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
            var order = await _unitOfWork.OrderRepository.GetAsync(expression: x => !x.IsDeleted && x.OrderStatus == OrderStatus.Failed, isDisableTracking: true, isTakeAll: true, expressionInclude: x => x.OrderDetails);
            if (order == null || order.TotalItemsCount <= 0)
                throw new Exception("Không tìm thấy đơn hàng.");
            foreach (var item in order.Items.FirstOrDefault().OrderDetails)
            {
                var product = await _unitOfWork.ProductRepository.GetByIdAsync(item.ProductId);
                if (product == null)
                    throw new Exception("Không tìm thấy sản phẩm bạn muốn mua");

                //cộng quantity của product
                product.Quantity += item.Quantity;
                _unitOfWork.ProductRepository.Update(product);
                await _unitOfWork.SaveChangeAsync();
            }
        }
    }
}

