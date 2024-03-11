using Application.ViewModels.BonsaiViewModel;
using Application.ViewModels.UserViewModels;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validations.Bonsai
{
    public class BonsaiModelValidator : AbstractValidator<BonsaiModel>
    {
        public BonsaiModelValidator()
        {
            RuleFor(x => x.CategoryId).NotEmpty().WithMessage("Category không được để trống.");

            RuleFor(x => x.StyleId).NotEmpty().WithMessage("StyleId không được để trống.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name không được để trống.")
                .MaximumLength(50).WithMessage("Name không quá 50 ký tự.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description không được để trống.");

            RuleFor(x => x.YearOfPlanting)
                .GreaterThan(0).WithMessage("AgeRange phải là số nguyên dương.");

            RuleFor(x => x.Height)
                .GreaterThan(0).WithMessage("Height phải là số dương.");

            RuleFor(x => x.TrunkDimenter)
               .GreaterThan(0).WithMessage("Unit không quá 20 ký tự.");

            RuleFor(x => x.NumberOfTrunk)
                .GreaterThan(0).WithMessage("Quantity phải là số nguyên dương.");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("UnitPrice phải là số dương.");
        }
    }
}
