using Application.ViewModels.OrderViewModels;
using FluentValidation;

namespace Application.Validations.Order
{
    public class OrderModelValidator : AbstractValidator<OrderModel>
    {
        public OrderModelValidator()
        {
            RuleFor(x => x.Address).NotEmpty().WithMessage("Địa chỉ chi tiết không được để trống!").MaximumLength(100)
                .WithMessage("Địa chỉ chi tiết không quá 100 ký tự.");
            RuleFor(x => x.ExpectedDeliveryDate).NotEmpty().WithMessage("Email không được để trống.")
                .GreaterThan(DateTime.Now)
                .WithMessage("Ngày nhận hàng phải lớn hơn này hiện tại.");
        }
    }
}
