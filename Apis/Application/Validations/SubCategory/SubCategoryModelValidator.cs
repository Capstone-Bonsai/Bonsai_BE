using Application.ViewModels.SubCategoryViewModels;
using FluentValidation;

namespace Application.Validations.SubCategory
{
    public class SubCategoryModelValidator : AbstractValidator<SubCategoryModel>
    {
        public SubCategoryModelValidator()
        {
            RuleFor(tag => tag.Name)
           .NotEmpty().WithMessage("Tên danh mục không được để trống.");
        }
    }
}
