﻿using Application.Interfaces;
using Application.Repositories;
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
        [HttpPost]
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
        public async Task<IActionResult> ChangeProfile([FromForm] UserRequestModel model)
        {
            string userId = _claims.GetCurrentUserId.ToString().ToLower();
            try
            {
                var result = await _userService.UpdateUserAsync(model, userId);
                if (result == null)
                {
                    return Ok("Thay đổi thông tin cá nhân thành công!");
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
        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            string userId = _claims.GetCurrentUserId.ToString().ToLower();
            try
            {
                var user = await _userService.GetUserByIdAsync( userId);
                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Manager")]
        [HttpGet]
        public async Task<IActionResult> GetListUser(int pageIndex= 0, int pageSize = 20)
        {
            try
            {
                var users = await _userService.GetListUserAsync(pageIndex, pageSize);
                return Ok(users);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Manager")]
        [HttpPut]
        public async Task<IActionResult> LockOutAsync(string userId)
        {
            try
            {
               var result = await _userService.LockOrUnlockUser(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize(Roles = "Manager")]
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromForm]UserCreateModel model)
        {
            try
            {
                var result = await _userService.CreateUserAccount(model);
                if(result == null)
                    return Ok("Tạo tài khoản thành công");
                else return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize(Roles = "Manager")]
        [HttpGet]
        public async Task<IActionResult> GetListRole()
        {
            try
            {
                var roles = await _userService.GetListRoleAsync();
                return Ok(roles);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}