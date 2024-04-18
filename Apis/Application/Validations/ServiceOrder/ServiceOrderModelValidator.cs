
using Application.ViewModels.ServiceOrderViewModels;
using Application.ViewModels.ServiceViewModels;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validations.ServiceOrder
{
    public class ServiceOrderModelValidator : AbstractValidator<ServiceOrderModel>
    {
        public ServiceOrderModelValidator()
        {
            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("Ngày bắt đầu không được để trống.")
                .GreaterThanOrEqualTo(DateTime.Today.AddDays(5)).WithMessage("Ngày bắt đầu phải ít nhất là 5 ngày kể từ hôm nay.");

            RuleFor(x => x.EndDate)
                .NotEmpty().WithMessage("Ngày kết thúc không được để trống.")
                .GreaterThanOrEqualTo(x => x.StartDate).WithMessage("Ngày kết thúc phải lớn hơn hoặc bằng Ngày bắt đầu.");
        }
    }
}
