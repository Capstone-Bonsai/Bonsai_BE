using Application.Interfaces;
using Application.Services.Momo;
using Application.ViewModels.OrderViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IClaimsService _claimsService;

        public OrderController(IOrderService orderService, IClaimsService claimsService)
        {
            _orderService = orderService;
            _claimsService = claimsService;
        }
        [HttpPost]
        public async Task<IActionResult> CreateOrderAsync([FromBody]OrderModel model)
        {
            try
            {
                var userId = _claimsService.GetCurrentUserId.ToString().ToLower();
                var resultValidate = await _orderService. ValidateOrderModel(model, userId);
                if (resultValidate == null)
                {
                    var result = await _orderService.CreateOrderAsync(model, userId);
                    if (result!=null)
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
        public async Task<IActionResult> GetAsync([FromQuery] int pageIndex, int pageSize)
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


        [HttpGet("{orderId}")]
        [Authorize]
        public async Task<IActionResult> GetByIdAsync( Guid orderId)
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



    }
}
