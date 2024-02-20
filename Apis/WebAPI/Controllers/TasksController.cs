using Application.Interfaces;
using Application.Services;
using Application.ViewModels.CategoryViewModels;
using Application.ViewModels.TasksViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly ITasksService _tasksService;
        private readonly IClaimsService _claims;

        public TasksController(ITasksService tasksService,
            IClaimsService claimsService)
        {
            _tasksService = tasksService;
            _claims = claimsService;
        }
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var tasks = await _tasksService.GetTasks();
                if (tasks.Items.Count == 0)
                {
                    return BadRequest("Không tìm thấy");
                }
                else
                {
                    return Ok(tasks);
                }
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
                var tasks = await _tasksService.GetTaskById(id);
                if (tasks == null)
                {
                    return BadRequest("Không tìm thấy");
                }
                else
                {
                    return Ok(tasks);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] TasksModel tasksModel)
        {
            try
            {
                await _tasksService.AddTask(tasksModel);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok();
        }
        [HttpPut("{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Put([FromRoute] Guid id, [FromBody] TasksModel tasksModel)
        {
            try
            {
                await _tasksService.UpdateTask(id, tasksModel);
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
                await _tasksService.DeleteTask(id);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok();
        }
    }
}
