using Application.ViewModels.ProductViewModels;
using Application.ViewModels.TagViewModels;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validations.Tag
{
    public class TagModelValidator : AbstractValidator<TagModel>
    {
        public TagModelValidator()
        {
            RuleFor(tag => tag.Name)
           .NotEmpty().WithMessage("Tên nhãn không được để trống.");

        }
    }
}
