using Application.Interfaces;
using Application.Repositories;
using Application.Validations.Order;
using Application.ViewModels.OrderViewModels;

namespace Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly ITransactionRepository _transactionRepository;

        public OrderService(ITransactionRepository transactionRepository)
        {
            _transactionRepository = transactionRepository;

        }
        public OrderService() { }
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


        public async Task<IList<string>> CreateOrderAsync(OrderModel model, string userId)
        {
            var resultValidate = await ValidateOrderModel(model, userId);
            if (resultValidate == null)
            {
                await _transactionRepository.CreateOrderByTransaction(model, userId);
                return null;
            }
            else
            {
                return resultValidate;
            }
        }

    }
}
