using Application.Interfaces;
using Application.Services;
using Application.ViewModels.BaseTaskViewTasks;
using Application.ViewModels.StyleViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseTaskController : ControllerBase
    {
        private readonly IBaseTaskService _baseTaskService;
        private readonly IClaimsService _claims;

        public BaseTaskController(IBaseTaskService baseTaskService,
            IClaimsService claimsService)
        {
            _baseTaskService = baseTaskService;
            _claims = claimsService;
        }
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var baseTasks = await _baseTaskService.GetBaseTasks();
                return Ok(baseTasks);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("Pagination")]
        public async Task<IActionResult> GetPagination(int page = 0, int size = 10)
        {
            try
            {
                var baseTasks = await _baseTaskService.GetBaseTasksPagination(page, size);
                return Ok(baseTasks);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            try
            {
                var style = await _baseTaskService.GetBaseTaskById(id);
                if (style == null)
                {
                    return BadRequest("Không tìm thấy");
                }
                else
                {
                    return Ok(style);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Post([FromBody] BaseTaskModel model)
        {
            try
            {
                var errs = await _baseTaskService.AddBaseTask(model);
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
        public async Task<IActionResult> Put([FromRoute] Guid id, [FromBody] BaseTaskModel model)
        {
            try
            {
                var errs = await _baseTaskService.UpdateBaseTask(id, model);
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
                await _baseTaskService.DeleteBaseTask(id);
                return Ok("Xóa thành công");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
