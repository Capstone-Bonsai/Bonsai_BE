using Application.Interfaces;
using Application.ViewModels.ServiceSurchargeViewModels;
using Application.ViewModels.StyleViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Numerics;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceSurchargeController : ControllerBase
    {
        private readonly IServiceSurchargeService _serviceSurchargeService;
        private readonly IClaimsService _claims;

        public ServiceSurchargeController(IServiceSurchargeService serviceSurchargeService,
            IClaimsService claimsService)
        {
            _serviceSurchargeService = serviceSurchargeService;
            _claims = claimsService;
        }
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var styles = await _serviceSurchargeService.Get();
                if (styles.Items.Count == 0)
                {
                    return BadRequest("Không tìm thấy");
                }
                else
                {
                    return Ok(styles);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("{distance}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Post([FromRoute] float distance)
        {
            try
            {
                var price = await _serviceSurchargeService.GetPriceByDistance(distance);
                return Ok(price);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
        [HttpPost]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Post([FromBody] ServiceSurchargeModel serviceSurchargeModel)
        {
            try
            {
                await _serviceSurchargeService.Add(serviceSurchargeModel);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok();
        }
        [HttpPut("{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Put([FromRoute] Guid id, [FromBody] ServiceSurchargeModel serviceSurchargeModel)
        {
            try
            {
                await _serviceSurchargeService.Edit(id, serviceSurchargeModel);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok();
        }
        [HttpDelete("{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            try
            {
                await _serviceSurchargeService.Delete(id);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok();
        }
    }
}
