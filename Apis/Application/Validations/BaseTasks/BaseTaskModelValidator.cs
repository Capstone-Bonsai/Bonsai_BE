using Application.ViewModels.BaseTaskViewTasks;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validations.BaseTasks
{
    public class BaseTaskModelValidator : AbstractValidator<BaseTaskModel>
    {
        public BaseTaskModelValidator()
        {
            RuleFor(x => x.Name)
           .NotEmpty().WithMessage("Tên nhiệm vụ không được để trống.").MaximumLength(50).WithMessage("Tên nhiệm vụ không quá 50 ký tự");
            RuleFor(x=>x.Detail).NotEmpty().WithMessage("Mô tả không được để trống.").MaximumLength(200).WithMessage("Mô tả không quá 200 ký tự");
        }
    }
}

