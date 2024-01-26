using Application.Interfaces;
using Application.Services.Momo;
using Application.ViewModels.OrderViewModels;
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
        public async Task<IActionResult> CreateOrderAsync(OrderModel model)
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


    }
}
