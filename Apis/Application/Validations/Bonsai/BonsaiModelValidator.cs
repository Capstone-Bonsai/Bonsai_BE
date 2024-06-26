﻿using Application.ViewModels.BonsaiViewModel;
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
            RuleFor(x => x.CategoryId).NotEmpty().WithMessage("Danh mục không được để trống.");

            RuleFor(x => x.StyleId).NotEmpty().WithMessage("Kiểu dáng không được để trống.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Tên không được để trống.")
                .MaximumLength(100).WithMessage("Tên không quá 100 ký tự.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Mô tả không được để trống.")
                .MaximumLength(2000).WithMessage("Mô tả không được quá 2000 ký tự.");

            RuleFor(x => x.YearOfPlanting)
                .GreaterThan(0).WithMessage("Năm trồng phải là số nguyên dương.");

            RuleFor(x => x.Height)
                .GreaterThan(0).WithMessage("Chiều cao phải là số dương.");

            RuleFor(x => x.TrunkDimenter)
                .NotEmpty().WithMessage("Hoành cây không được để trống.")
               .GreaterThan(0).WithMessage("Hoành cây phải là số dương.");

            RuleFor(x => x.NumberOfTrunk)
                 .NotEmpty().WithMessage("Số thân không được để trống.")
                .GreaterThan(0).WithMessage("Số thân phải là số nguyên dương.");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Giá phải là số dương.");
        }
    }
}
