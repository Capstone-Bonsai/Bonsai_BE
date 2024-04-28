using Application.Interfaces;
using Application.Services.Momo;
using Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Domain.Enums;
using Application.ViewModels.ServiceOrderViewModels;

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
        [HttpGet("Pagination")]
        [Authorize]
        public async Task<IActionResult> Get([FromQuery] int pageIndex, int pageSize)
        {
            try
            {
                var contracts = await _serviceOrderService.GetServiceOrders(pageIndex, pageSize, _claims.GetIsCustomer, _claims.GetCurrentUserId);
                if (contracts.Items.Count == 0)
                {
                    return BadRequest("Không tìm thấy");
                }
                else
                {
                    return Ok(contracts);
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Post([FromBody] ServiceOrderModel serviceOrderModel)
        {
            try
            {
                await _serviceOrderService.CreateServiceOrder(serviceOrderModel);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPut("Update/{serviceOrderId}")]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> Put(Guid serviceOrderId,[FromBody] ResponseServiceOrderModel responseServiceOrderModel)
        {
            try
            {
                await _serviceOrderService.UpdateServiceOrder(serviceOrderId, responseServiceOrderModel);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("WorkingCalendar")]
        [Authorize(Roles = "Gardener")]
        public async Task<IActionResult> GetWorkingCalendar(int month, int year)
        {
            try
            {
                var contracts = await _serviceOrderService.GetWorkingCalendar(month, year, _claims.GetCurrentUserId);
                return Ok(contracts);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("Gardener/{serviceOrderId}")]
        [Authorize(Roles = "Gardener")]
        public async Task<IActionResult> GetByIdForGardener(Guid serviceOrderId)
        {
            try
            {
                var serviceOrder = await _serviceOrderService.GetServiceOrderByIdForGardener(serviceOrderId);
                return Ok(serviceOrder);
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
                await _serviceOrderService.HandleIpnAsync(momo);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("Payment")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Payment(Guid ContractId)
        {
            try
            {
                Guid userId = _claims.GetCurrentUserId;

                var linkPayment = await _serviceOrderService.PaymentContract(ContractId, userId.ToString().ToLower());
                if (linkPayment == null)
                    return BadRequest("Không tìm thấy");
                else
                    return Ok(linkPayment);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("{serviceOrderId}")]
        [Authorize]
        public async Task<IActionResult> GetById(Guid serviceOrderId)
        {
            try
            {
                var contracts = await _serviceOrderService.GetServiceOrderById(serviceOrderId, _claims.GetIsCustomer, _claims.GetCurrentUserId);
                return Ok(contracts);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("AddContract/{serviceOrderId}")]
        [Authorize(Roles = "Staff,Manager")]
        public async Task<IActionResult> AddIMage([FromRoute] Guid serviceOrderId, [FromForm] ServiceOrderImageModel serviceOrderImageModel)
        {
            try
            {
                await _serviceOrderService.AddContract(serviceOrderId, serviceOrderImageModel);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{serviceOrderId}")]
        [Authorize(Roles = "Manager,Staff")]
        [Authorize]
        public async Task<IActionResult> UpdateStatusAsync(Guid serviceOrderId, ServiceOrderStatus serviceOrderStatus)
        {
            try
            {
                await _serviceOrderService.UpdateServiceOrderStatus(serviceOrderId, serviceOrderStatus);
                return Ok("Cập nhật trạng thái hóa đơn thành công.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
