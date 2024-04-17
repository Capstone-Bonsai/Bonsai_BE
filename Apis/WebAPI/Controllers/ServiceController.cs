using Application.Interfaces;
using Application.Services;
using Application.ViewModels;
using Application.ViewModels.BaseTaskViewTasks;
using Application.ViewModels.ServiceViewModels;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Drawing.Printing;
using WebAPI.Services;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceController : ControllerBase
    {

        private readonly IServiceService _serviceService;
        private readonly IClaimsService _claims;

        public ServiceController(IServiceService serviceService,
            IClaimsService claimsService)
        {
            _serviceService = serviceService;
            _claims = claimsService;
        }
        [HttpGet("Package")]
        public async Task<IActionResult> GetPackage(int pageIndex, int pageSize, Guid serviceTypeId, Guid? customerBonsaiId)
        {
            try
            {
                var userId = _claims.GetCurrentUserId.ToString().ToLower();
                var service = await _serviceService.GetServicePagination(pageIndex, pageSize, serviceTypeId, customerBonsaiId);
                return Ok(service);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetPagination(int pageIndex = 0, int pageSize = 10)
        {
            try
            {
                var userId = _claims.GetCurrentUserId.ToString().ToLower();
                var service = await _serviceService.GetServicePagination(userId, pageIndex, pageSize);
                return Ok(service);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        /*[HttpGet("ServiveType")]
         public async Task<IActionResult> ServiveType()
         {
             try
             {
                 List<EnumModel> enums = ((ServiceType[])Enum.GetValues(typeof(ServiceType))).Select(c => new EnumModel() { Value = (int)c, Display = c.ToString() }).ToList();
                 return Ok(enums);

             }
             catch (Exception ex)
             {
                 return BadRequest(ex.Message);
             }
         }*/

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            try
            {
                var userId = _claims.GetCurrentUserId.ToString().ToLower();
                var service = await _serviceService.GetServiceById(id, userId);
                if (service == null)
                {
                    return BadRequest("Không tìm thấy");
                }
                else
                {
                    return Ok(service);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Post([FromForm] ServiceModel model)
        {
            try
            {
                var errs = await _serviceService.AddService(model);
                if (errs == null)
                    return Ok("Tạo mới thành công.");
                else
                    return BadRequest(errs);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Put(Guid id, [FromForm] ServiceModel model)
        {
            try
            {
                var errs = await _serviceService.UpdateService(id, model);
                if (errs == null)
                    return Ok("Cập nhật thành công.");
                else
                    return BadRequest(errs);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpDelete("{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            try
            {
                await _serviceService.DeleteService(id);
                return Ok("Xóa thành công");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
