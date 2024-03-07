/*using Application.ViewModels.ProductViewModels;
using Application.ViewModels.UserViewModels;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validations.Product
{
    public class ProductModelValidator : AbstractValidator<BonsaiModel>
    {
        public ProductModelValidator()
        {
            RuleFor(x => x.SubCategoryId).NotEmpty().WithMessage("SubCategoryId không được để trống.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name không được để trống.")
                .MaximumLength(50).WithMessage("Name không quá 50 ký tự.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description không được để trống.");

            RuleFor(x => x.TreeShape)
                .MaximumLength(50).WithMessage("TreeShape không quá 50 ký tự.");

            RuleFor(x => x.AgeRange)
                .GreaterThan(0).WithMessage("AgeRange phải là số nguyên dương.");

            RuleFor(x => x.Height)
                .GreaterThan(0).WithMessage("Height phải là số dương.");

            RuleFor(x => x.Unit)
                .NotEmpty().WithMessage("Unit không được để trống.")
                .MaximumLength(20).WithMessage("Unit không quá 20 ký tự.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity phải là số nguyên dương.");

            RuleFor(x => x.UnitPrice)
                .GreaterThan(0).WithMessage("UnitPrice phải là số dương.");
        }
    }
}
*/