using Application.ViewModels.ServiceViewModels;
using Application.ViewModels.StyleViewModels;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validations.Style
{
    public class StyleModelValidator : AbstractValidator<StyleModel>
    {
        public StyleModelValidator()
        {
            RuleFor(style => style.Name)
           .NotEmpty().WithMessage("Tên kiểu dáng không được để trống.");
        }
    }
}
