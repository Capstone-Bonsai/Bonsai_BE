using Application.ViewModels.OrderViewModels;
using Application.ViewModels.ServiceViewModels;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validations.Services
{
    public class ServiceModelValidator : AbstractValidator<ServiceModel>
    {
        public ServiceModelValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Tên dịch vụ không được để trống.")
                .MaximumLength(50)
                .WithMessage("Tên dịch vụ không quá 50 ký tự.");
            RuleFor(x => x.Description).NotEmpty().WithMessage("Mô tả không được để trống.")
                .MaximumLength(1000)
                .WithMessage("Mô tả không quá 1000 ký tự.");
            RuleFor(x => x.StandardPrice).NotEmpty().WithMessage("Số điện thoại không được để trống.").GreaterThan(0).WithMessage("Giá dịch vụ phải lớn hơn 0");
              

        }
    }
}
