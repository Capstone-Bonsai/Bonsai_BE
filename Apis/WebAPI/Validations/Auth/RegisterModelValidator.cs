using Application.ViewModels.AuthViewModel;
using Domain.Entities;
using FluentValidation;
using Infrastructures;
using Microsoft.AspNetCore.Identity;

namespace WebAPI.Validations.Auth
{
    public class RegisterModelValidator : AbstractValidator<RegisterModel>
    {
        public RegisterModelValidator()
        {
            RuleFor(x => x.Username).NotEmpty().WithMessage("Tên đăng nhập không được để trống.")
                .WithMessage("Tên không quá 50 ký tự.");
            RuleFor(x => x.Email).NotEmpty().WithMessage("Email không được để trống.")
                .MaximumLength(50)
                .WithMessage("Email không quá 50 ký tự.");
            RuleFor(x => x.PhoneNumber).NotEmpty().WithMessage("Số điện thoại không được để trống.")
               .MaximumLength(10)
                .WithMessage("Số điện thoại phải có 10 ký tự.").MinimumLength(10)
                .WithMessage("Số điện thoại  phải có 10 ký tự.").MustAsync(IsPhoneNumberValid).WithMessage("Số điện thoại chỉ được chứa các chữ số.")
                .MustAsync(IsPhoneNumberStartWith).WithMessage("Số điện thoại chỉ được bắt đầu bằng các đầu số 03, 05, 07, 08, 09."); 
            RuleFor(x => x.Fullname).NotEmpty().WithMessage("Họ tên không được để trống.")
                .MaximumLength(50)
                .WithMessage("Họ tên ngắn không quá 50 ký tự.");

            RuleFor(x => x.Password).NotEmpty().WithMessage("Mật khẩu không được để trống.")
              .MaximumLength(50).WithMessage("Mật khẩu không quá 50 ký tự.");
        }

        public async Task<bool> IsPhoneNumberValid(string phoneNumber, CancellationToken cancellationToken)
        {
            foreach (char c in phoneNumber)
            {
                if (!char.IsDigit(c))
                {
                    return false;
                }
            }
            return true;
        }

        public async Task<bool> IsPhoneNumberStartWith(string phoneNumber, CancellationToken cancellationToken)
        {
            if(phoneNumber.StartsWith("08") || phoneNumber.StartsWith("09") || phoneNumber.StartsWith("03") || phoneNumber.StartsWith("07") || phoneNumber.StartsWith("05"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
