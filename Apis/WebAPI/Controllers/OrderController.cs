using Application.Interfaces;
using Application.Services.Momo;
using Application.Validations.Auth;
using Application.Validations.Order;
using Application.ViewModels;
using Application.ViewModels.AuthViewModel;
using Application.ViewModels.OrderViewModels;
using Domain.Entities;
using Domain.Enums;
using Firebase.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IClaimsService _claimsService;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(IOrderService orderService, IClaimsService claimsService,UserManager<ApplicationUser> userManager)
        {
            _orderService = orderService;
            _claimsService = claimsService;
            _userManager = userManager;
        }
        [HttpPost]
        public async Task<IActionResult> CreateOrderAsync([FromBody] OrderModel model)
        {
            try
            {
                var userId = _claimsService.GetCurrentUserId.ToString().ToLower();
                var resultValidate = await _orderService.ValidateOrderModel(model, userId);
                if (resultValidate == null)
                {
                    var result = await _orderService.CreateOrderAsync(model, userId);
                    if (result != null)
                        return Ok(result);
                    else return BadRequest(result);
                }
                else
                {
                    return BadRequest(resultValidate);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPost("IpnHandler")]
        public async Task<IActionResult> IpnAsync([FromBody] MomoRedirect momo)
        {
            try
            {
                await _orderService.HandleIpnAsync(momo);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAsync([FromQuery] int pageIndex, [FromQuery] int pageSize)
        {
            try
            {
                var userId = _claimsService.GetCurrentUserId.ToString().ToLower();
                var orders = await _orderService.GetPaginationAsync(userId, pageIndex, pageSize);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }


        [HttpPost("OTPGeneration")]
        public async Task<IActionResult> GenerateTokenAsync([FromForm] OrderInfoModel model)
        {
            var validateError = await ValidateAsync(model);
            if (validateError == null)
            {
                try
                {
                    await _orderService.GenerateTokenAsync(model);
                    return Ok("Mã xác thực đã được gửi về email. Vui lòng truy cập email để nhận mã OTP");
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
            else
            {
                return BadRequest(validateError);
            }
        }

        [HttpGet("{orderId}")]
        [Authorize]
        public async Task<IActionResult> GetByIdAsync(Guid orderId)
        {
            try
            {
                var userId = _claimsService.GetCurrentUserId.ToString().ToLower();
                var orders = await _orderService.GetByIdAsync(userId, orderId);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [Authorize(Roles = "Manager,Staff,Gardener")]
        [HttpPut("{orderId}")]
        [Authorize]
        public async Task<IActionResult> UpdateStatusAsync(Guid orderId, OrderStatus orderStatus)
        {
            try
            {
                await _orderService.UpdateOrderStatusAsync(orderId, orderStatus);
                return Ok("Cập nhật trạng thái đơn hàng thành công.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Manager,Staff")]
        [HttpGet("OrderStatus")]
        [Authorize]
        public async Task<IActionResult> GetOrderStatusAsync()
        {
            try
            {
                List<EnumModel> enums = ((OrderStatus[])Enum.GetValues(typeof(OrderStatus))).Select(c => new EnumModel() { Value = (int)c, Display = c.ToString() }).ToList();
                return Ok(enums);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Gardener")]
        [HttpPut("DeliveryFinishing/{orderId}")]
        [Authorize]
        public async Task<IActionResult> FinishDeliveryOrder(Guid orderId, [FromForm] FinishDeliveryOrderModel finishDeliveryOrderModel)
        {
            try
            {
                await _orderService.FinishDeliveryOrder(orderId, finishDeliveryOrderModel);
                return Ok("Cập nhật trạng thái đơn hàng thành công.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        //[Authorize(Roles = "Manager")]
        [HttpPut("AddGardener/{orderId}")]
        //[Authorize]
        public async Task<IActionResult> FinishDeliveryOrder(Guid orderId, Guid gardenerId)
        {
            try
            {
                await _orderService.AddGardenerForOrder(orderId, gardenerId);
                return Ok("Cập nhật trạng thái đơn hàng thành công.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [HttpPost("SendNotification")]
        //[Authorize(Roles = "Customer")]
        public async Task<IActionResult> CreateNotificationForStaff(Guid orderId)
        {
            try
            {
                await _orderService.CreateNotificationForStaff(_claimsService.GetCurrentUserId, orderId);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [NonAction]
        private async Task<IList<string>> ValidateAsync(OrderInfoModel model)
        {
            var validator = new OrderInfoModelValidator();
            var result = await validator.ValidateAsync(model);
            if (!result.IsValid)
            {
                var errors = new List<string>();
                errors.AddRange(result.Errors.Select(x => x.ErrorMessage));
                return errors;
            }
            return null;
        }


        [HttpGet("OtpHandler")]
        public async Task<IActionResult> OtpHandler(string Email, string Otp)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(Email);
                if (user == null)
                {
                    return NotFound("Không tìm thấy người dùng.");
                }
                var result = await _userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultPhoneProvider, Otp);
                if (result)
                {
                    return Ok("Mã OTP chính xác");
                }
                else
                {
                    return BadRequest("Mã OTP không chính xác");
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
