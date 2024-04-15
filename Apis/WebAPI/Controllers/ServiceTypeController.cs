using Application.Interfaces;
using Application.Services;
using Application.ViewModels.BonsaiViewModel;
using Application.ViewModels.ServiceTypeViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceTypeController : ControllerBase
    {
        private readonly IServiceTypeService _serviceTypeService;
        private readonly IClaimsService _claims;

        public ServiceTypeController(IServiceTypeService serviceTypeService,
            IClaimsService claimsService)
        {
            _serviceTypeService = serviceTypeService;
            _claims = claimsService;
        }
        [HttpGet]
        public async Task<IActionResult> GetPagination()
        {
            try
            {
                var serviceTypes = await _serviceTypeService.Get();
                if (serviceTypes.Items.Count == 0)
                {
                    return BadRequest("Không tìm thấy!");
                }
                else
                {
                    return Ok(serviceTypes);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] Guid id, [FromForm] ServiceTypeModel serviceTypeModel)
        {
            try
            {
                await _serviceTypeService.Update(id, serviceTypeModel);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok("Cập nhật thành công!");
        }
    }
}
