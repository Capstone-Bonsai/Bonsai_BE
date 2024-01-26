using Application.ViewModels.CategoryViewModels;
using Application.ViewModels.TagViewModels;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validations.Category
{
    public class CategoryModelValidator : AbstractValidator<CategoryModel>
    {
        public CategoryModelValidator()
        {
            RuleFor(tag => tag.Name)
           .NotEmpty().WithMessage("Tên danh mục không được để trống.");
        }
    }
}
