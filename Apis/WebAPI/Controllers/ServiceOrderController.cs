using Application.Interfaces;
using Application.Services;
using Application.ViewModels.ServiceModels;
using Application.ViewModels.ServiceOrderViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceOrderController : ControllerBase
    {
        private readonly IServiceOrderService _serviceOrderService;
        private readonly IClaimsService _claims;

        public ServiceOrderController(IServiceOrderService serviceOrderService,
            IClaimsService claimsService)
        {
            _serviceOrderService = serviceOrderService;
            _claims = claimsService;
        }
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var serviceOrders = await _serviceOrderService.GetServiceOrders();
                if (serviceOrders.Items.Count == 0)
                {
                    return BadRequest("Không tìm thấy");
                }
                else
                {
                    return Ok(serviceOrders);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> Get([FromRoute] Guid id)
        {
            try
            {
                var serviceOrder = await _serviceOrderService.GetOrderServiceById(id);
                if (serviceOrder == null)
                {
                    return BadRequest("Không tìm thấy");
                }
                else
                {
                    return Ok(serviceOrder);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Post([FromForm] ServiceOrderModel serviceOrderModel)
        {
            try
            {
                await _serviceOrderService.AddServiceOrder(serviceOrderModel, _claims.GetCurrentUserId);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok();
        }
        [HttpPut("{id}")]
        [Authorize(Roles = "Manager,Staff")]
        public async Task<IActionResult> Put([FromRoute] Guid id, [FromBody] ResponseServiceOrderModel responseServiceOrderModel)
        {
            try
            {
                await _serviceOrderService.ResponseServiceOrder(id, responseServiceOrderModel);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok();
        }
        [HttpGet("ServicePayment")]
        public async Task<IActionResult> PaymentAsync(Guid serviceOrderId)
        {
            try
            {
                var serviceOrders = await _serviceOrderService.PaymentAsync(serviceOrderId);
                return Ok(serviceOrders);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
