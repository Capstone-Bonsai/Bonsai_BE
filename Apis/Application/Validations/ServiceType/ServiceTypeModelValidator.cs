using Application.ViewModels.ServiceTypeViewModels;
using Application.ViewModels.ServiceViewModels;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validations.ServiceType
{
    public class ServiceTypeModelValidator : AbstractValidator<ServiceTypeModel>
    {
        public ServiceTypeModelValidator()
        {
            RuleFor(x => x.Description).NotEmpty().WithMessage("Mô tả không được để trống.")
                .MaximumLength(1000)
                .WithMessage("Mô tả không quá 1000 ký tự.");
            RuleFor(x => x.Image)
    .NotEmpty()
    .WithMessage("Ảnh không được để trống")
    .Must(image => image != null)
    .WithMessage("Chỉ một ảnh được cho phép");
        }
    }
}
