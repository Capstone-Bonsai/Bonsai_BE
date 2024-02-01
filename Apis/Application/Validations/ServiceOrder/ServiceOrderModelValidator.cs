using Application.ViewModels.ServiceOrderViewModels;
using FluentValidation;

namespace Application.Validations.ServiceOrder
{
    public class ServiceOrderModelValidator : AbstractValidator<ServiceOrderModel>
    {
        public ServiceOrderModelValidator()
        {
            RuleFor(order => order.Address)
                .NotEmpty().WithMessage("Address is required.");

            RuleFor(order => order.StartDate)
                .NotEmpty().WithMessage("Start date is required.");

            RuleFor(order => order.EndDate)
                .NotEmpty().WithMessage("End date is required.")
                .GreaterThan(order => order.StartDate).WithMessage("End date must be greater than start date.");

            RuleFor(order => order.GardenSquare)
                .GreaterThan(0).WithMessage("Garden square must be greater than 0.");

            RuleFor(order => order.ExpectedWorkingUnit)
                .GreaterThan(0).WithMessage("Expected working unit must be greater than 0.");

            RuleFor(order => order.TemporaryPrice)
                .GreaterThan(0).WithMessage("Temporary price must be greater than 0.");

            RuleFor(order => order.TemporaryTotalPrice)
                .GreaterThan(0).WithMessage("Temporary total price must be greater than 0.");


        }
    }
}
