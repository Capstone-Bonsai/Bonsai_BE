using Application.Interfaces;
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
                var result = await _orderService.CreateOrderAsync(model, userId);
                if (result == null)
                {
                    return Ok();
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
