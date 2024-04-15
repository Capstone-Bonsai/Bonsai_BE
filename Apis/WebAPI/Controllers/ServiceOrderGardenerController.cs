using Application.Interfaces;
using Application.Services;
using Application.ViewModels.ServiceOrderViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceOrderGardenerController : ControllerBase
    {
        private readonly IServiceOrderGardenerService _serviceOrderGardenerService;
        private readonly IClaimsService _claims;

        public ServiceOrderGardenerController(IServiceOrderGardenerService serviceOrderGardenerService,
            IClaimsService claimsService)
        {
            _serviceOrderGardenerService = serviceOrderGardenerService;
            _claims = claimsService;
        }
        [HttpGet("{serviceOrderId}")]
        public async Task<IActionResult> GetGardenerOfServiceOrder([FromQuery] int pageIndex, int pageSize,[FromRoute] Guid serviceOrderId)
        {
            try
            {
                var users = await _serviceOrderGardenerService.GetGardenerOfServiceOrder(pageIndex, pageSize, serviceOrderId);
                return Ok(users);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ServiceOrderGardenerModel serviceOrderGardenerModel)
        {
            try
            {
                await _serviceOrderGardenerService.AddGardener(serviceOrderGardenerModel);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
