﻿using Application.ViewModels.OrderDetailModels;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validations.Order
{
    public class OrderDetailModelValidator : AbstractValidator<OrderDetailModel>
    {
        public OrderDetailModelValidator()
        {
            RuleFor(x => x.ProductId).NotEmpty().WithMessage("Sản phẩm không được để trống!");
            RuleFor(x => x.Quantity).GreaterThan(0)
                .WithMessage("Số lượng phải lớn hơn 0.");
        }
    }
}