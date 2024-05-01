using Application.ViewModels.BonsaiViewModel;
using Application.ViewModels.CustomerBonsaiViewModels;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validations.Bonsai
{
    public class BonsaiModelForCustomerValidator : AbstractValidator<BonsaiModelForCustomer>
    {
        public BonsaiModelForCustomerValidator()
        {
            RuleFor(x => x.Address).MaximumLength(200).WithMessage("Địa chỉ không quá 200 ký tự.");
            RuleFor(x => x.Square).GreaterThan(0).WithMessage("Diện tích vườn lớn hơn 0.");
            RuleFor(x => x.CategoryId).NotEmpty().WithMessage("Danh mục không được để trống.");

            RuleFor(x => x.StyleId).NotEmpty().WithMessage("Kiểu dáng không được để trống.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Tên không được để trống.")
                .MaximumLength(1000).WithMessage("Tên không quá 100 ký tự.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Mô tả không được để trống.")
                .MaximumLength(2000).WithMessage("Mô tả không quá 2000 ký tự.");

            RuleFor(x => x.YearOfPlanting)
                .GreaterThan(0).WithMessage("Năm phải là số nguyên dương.");

            RuleFor(x => x.Height)
                .GreaterThan(0).WithMessage("Chiều cao phải là số dương.");

            RuleFor(x => x.TrunkDimenter)
               .GreaterThan(0).WithMessage("Hoành cây phải là số dương.");

            RuleFor(x => x.NumberOfTrunk)
                .GreaterThan(0).WithMessage("Số thân phải là số nguyên dương.");
        }
    }
}
