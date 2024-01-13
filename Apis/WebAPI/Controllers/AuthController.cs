using Application;
using Application.Commons;
using Application.Interfaces;
using Application.Services;
using Application.ViewModels;
using Application.ViewModels.AuthViewModel;
using Domain.Entities;
using Infrastructures;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using WebAPI.Validations.Auth;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IUserClaimsPrincipalFactory<ApplicationUser> _userClaimsPrincipalFactory;
        private readonly IAuthorizationService _authorizationService;
        private readonly IConfiguration _configuration;
        public readonly IWebHostEnvironment _environment;
        private readonly IUnitOfWork _unit;

        public AuthController(UserManager<ApplicationUser> userManager,
             SignInManager<ApplicationUser> signInManager,
            IUserClaimsPrincipalFactory<ApplicationUser> userClaimsPrincipalFactory,
            IAuthorizationService authorizationService,
            IConfiguration configuration,
            IWebHostEnvironment environment, IUnitOfWork unit)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
            _authorizationService = authorizationService;
            _configuration = configuration;
            _environment = environment;
            _unit = unit;
        }
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var _auth = new AuthService(_userManager, _signInManager, _configuration, _environment, _unit);
            try
            {
                //var result = await _identityService.AuthenticateAsync(email, password);

                var user = await _userManager.FindByNameAsync(model.Email);
                if (user == null)
                {
                    user = await _userManager.FindByEmailAsync(model.Email);
                    if (user == null)
                    {
                        return NotFound("Tài khoản này không tồn tại!");
                    }
                }

                string callbackUrl = "";
                //lấy host để redirect về
                var referer = Request.Headers["Referer"].ToString();
                string schema;
                string host;
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                if (Uri.TryCreate(referer, UriKind.Absolute, out var uri))
                {
                    schema = uri.Scheme; // Lấy schema (http hoặc https) của frontend
                    host = uri.Host; // Lấy host của frontend
                    callbackUrl = schema + "://" + host + Url.Action("ConfirmEmail", "Auth", new { userId = user.Id, code = code });
                }
                if (referer.Equals("https://localhost:5001/swagger/index.html"))
                {
                    callbackUrl = Request.Scheme + "://" + Request.Host + Url.Action("ConfirmEmail", "Auth", new { userId = user.Id, code = code });
                }
                //kết thúc lấy host để redirect về và tạo link


                //callbackUrl = Request.Scheme + "://" + Request.Host + Url.Action("ConfirmEmail", "Auth", new { userId = user.Id, code = code });
                var result = await _auth.Login(model.Email, model.Password, callbackUrl);
                if (result == null)
                {
                    return NotFound("Đăng nhập không thành công!");
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }

        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var _auth = new AuthService(_userManager, _signInManager, _configuration, _environment, _unit);
            try
            {
                var validator = new RegisterModelValidator();
                var result = await validator.ValidateAsync(model);
                if (!result.IsValid)
                {
                    ErrorViewModel errors = new ErrorViewModel();
                    errors.Errors = new List<string>();
                    errors.Errors.AddRange(result.Errors.Select(x => x.ErrorMessage));
                    return BadRequest(errors);
                }
                //check account
                await _auth.CheckAccountExist(model);
                //kết thúc lấy host để redirect về và tạo link
                var temp = await _auth.Register(model);
                if (temp == null)
                {

                    //lấy host để redirect về
                    var referer = Request.Headers["Referer"].ToString().Trim();
                    var callbackUrl = await GetCallbackUrlAsync(model.Email.Trim(), referer, "EmailConfirm");
                    await _auth.SendEmailAsync(model.Email.Trim(), callbackUrl, "EmailConfirm");
                    return Ok("Đăng ký tài khoản Thanh Sơn Garden thành công. Vui lòng kiểm tra email để kích hoạt tài khoản!");
                }
                else
                {
                    return BadRequest(temp);
                }
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string? code, string? userId)
        {
            var _auth = new AuthService(_userManager, _signInManager, _configuration, _environment, _unit);
            try
            {
                await _auth.ConfirmEmailAsync(code, userId);
                return Ok("Xác nhận Email thành công! Bây giờ bạn có thể đăng nhập vào tài khoản của mình bằng Email hoặc Username vừa xác thực !");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("Login/Google")]
        public async Task<IActionResult> LoginGoogle([FromBody] ExternalLoginModel model)
        {
            try
            {
                var _auth = new AuthService(_userManager, _signInManager, _configuration, _environment, _unit);
                var result = await _auth.HandleExternalLoginAsync(model);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPassModel model)
        {
            try
            {
                var _auth = new AuthService(_userManager, _signInManager, _configuration, _environment, _unit);
                var result = await _auth.ResetPasswordAsync(model);
                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest( e.Message);
            }
        }

        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return BadRequest("Không tìm thấy địa chỉ emal");
                }
                //lấy host để redirect về
                var _auth = new AuthService(_userManager, _signInManager, _configuration, _environment, _unit);
                var referer = Request.Headers["Referer"].ToString().Trim();
                var callbackUrl = await GetCallbackUrlAsync(email.Trim(), referer, "ResetPassword");
                await _auth.SendEmailAsync(email.Trim(), callbackUrl, "ResetPassword");
                return Ok("Yêu cầu đổi mật khẩu đã được gửi thành công đến địa chỉ email của bạn. Vui lòng kiểm tra hộp thư đến của bạn và xác thực email để tiến hành đổi mật khẩu.");
            }
            catch (Exception e)
            {
                return BadRequest("Xác nhận email không thành công: " + e.Message);
            }
        }

        [NonAction]
        public async Task<string> GetCallbackUrlAsync(string email, string referer, string type)
        {
            var user = await _userManager.FindByEmailAsync(email);
            string callbackUrl = "";
            string schema;
            string host;
            var code = "";
            var action = "";
            switch (type)
            {
                case "EmailConfirm":
                    code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    action = "ConfirmEmail";
                    break;

                case "ResetPassword":
                    code = await _userManager.GeneratePasswordResetTokenAsync(user);
                    action = "ResetPassword";
                    break;
            }

            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            if (!referer.Equals("") && Uri.TryCreate(referer, UriKind.Absolute, out var uri))
            {
                schema = uri.Scheme; // Lấy schema (http hoặc https) của frontend
                host = uri.Host; // Lấy host của frontend
                callbackUrl = schema + "://" + host + Url.Action(action, "Auth", new { userId = user.Id, code = code });
            }
            if (referer.Equals("https://localhost:5001/swagger/index.html"))
            {
                callbackUrl = "https://localhost:5001" + Url.Action(action, "Auth", new { userId = user.Id, code = code });
            }
            return callbackUrl;
        }

    }
}
