using Application.Interfaces;
using Application.Services;
using Application.ViewModels.OrderViewModels;
using Application.ViewModels.ServiceGardenViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Security.Claims;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceGardenController : ControllerBase
    {
        private readonly IServiceGardenService _serviceGardenService;
        private readonly IClaimsService _claimsService;

        public ServiceGardenController(IServiceGardenService serviceGardenService, IClaimsService claimsService)
        {
            _serviceGardenService = serviceGardenService;
            _claimsService = claimsService;
        }
        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> CreateOrderAsync([FromBody] ServiceGardenModel serviceGardenModel)
        {
            try
            {
                var result = await _serviceGardenService.AddServiceGarden(serviceGardenModel);
                if (result != null)
                    return Ok(result);
                else return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("customerGardenId")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetOrderAsync([FromQuery] int pageIndex, int pageSize, [FromBody] Guid customerGardenId)
        {
            try
            {
                var result = await _serviceGardenService.GetServiceGardenByGardenId(customerGardenId, pageIndex, pageSize);
                if (result != null)
                    return Ok(result);
                else return BadRequest(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPut("cancellation/{customerGardenId}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> CancelServiceGarden([FromBody] Guid customerGardenId)
        {
            try
            {
                await _serviceGardenService.CancelServiceGarden(customerGardenId);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPut("negation/{customerGardenId}")]
        [Authorize(Roles = "Staff,Manager")]
        public async Task<IActionResult> DenyServiceGarden([FromBody] Guid customerGardenId)
        {
            try
            {
                await _serviceGardenService.DenyServiceGarden(customerGardenId);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("Pagination")]
        public async Task<IActionResult> GetPagination([FromQuery] int pageIndex, int pageSize)
        {
            try
            {
                var pagination = await _serviceGardenService.GetServiceGarden(pageIndex, pageSize);
                if (pagination.Items.Count == 0)
                {
                    return BadRequest("Không tìm thấy!");
                }
                else
                {
                    return Ok(pagination);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
