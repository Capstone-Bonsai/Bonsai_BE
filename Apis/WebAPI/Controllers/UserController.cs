using Application.Interfaces;
using Application.ViewModels.UserViewModels;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    public class UserController : BaseController
    {
        private readonly IUserService _userService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IClaimsService _claims;

        public UserController(IUserService userService,
            UserManager<ApplicationUser> userManager,
             SignInManager<ApplicationUser> signInManager, IClaimsService claimsService)
        {
            _userService = userService;
            _userManager = userManager;
            _signInManager = signInManager;
            _claims = claimsService;
        }
        [Authorize]
        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePassModel model)
        {
            string userId = _claims.GetCurrentUserId.ToString().ToLower();
            try
            {
                var result = await _userService.ChangePasswordAsync(model, userId);
                if (result == null)
                {
                    return Ok("Đổi mật khẩu thành công");
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPut]
        public async Task<IActionResult> ChangeProfileAsync([FromForm] UserRequestModel model)
        {
            string userId = _claims.GetCurrentUserId.ToString().ToLower();
            try
            {
                var result = await _userService.UpdateUserAsync(model, userId);
                if (result == null)
                {
                    return Ok("Đổi mật khẩu thành công");
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}