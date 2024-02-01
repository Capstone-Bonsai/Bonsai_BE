using Application;
using Application.Commons;
using Application.Interfaces;
using Application.ViewModels.UserViewModels;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Infrastructures.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentTime _currentTime;
        private readonly AppConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IClaimsService _claims;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper, ICurrentTime currentTime, AppConfiguration configuration, UserManager<ApplicationUser> userManager, IClaimsService claims)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentTime = currentTime;
            _configuration = configuration;
            _userManager = userManager;
            _claims = claims;

        }
        public async Task<List<string>> ChangePasswordAsync(ChangePassModel model, string userId)
        {
            if (!model.NewPassword.Equals(model.ConfirmPassword))
            {
                throw new Exception("Mật khẩu xác nhận không trùng khớp!");

            }
            if (model.NewPassword.Equals(model.OldPassword))
            {
                throw new Exception("Mật khẩu mới phải khác mật khẩu cũ!");
            }
            try
            {

                var user = await _userManager.FindByIdAsync(userId);
                var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                if (result.Succeeded)
                {
                    return null;
                }
                else
                {
                    List<string> err = new List<string>();
                    foreach (var item in result.Errors)
                    {
                        err.Add(item.Description);
                    }
                    return err;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Đã xảy ra lỗi: {ex.Message}");
            }
        }

        public Task<List<string>> UpdateUserAsync(UserRequestModel model, string userId)
        {
            return null;
        }
    }
}
