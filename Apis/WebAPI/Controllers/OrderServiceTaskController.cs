using Application.Interfaces;
using Application.Services;
using Application.ViewModels.OrderServiceTaskModels;
using Application.ViewModels.TasksViewModels;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderServiceTaskController : ControllerBase
    {
        private readonly IOrderServiceTaskService _orderServiceTaskService;
        private readonly IClaimsService _claims;
        public OrderServiceTaskController(IOrderServiceTaskService orderServiceTaskService,
            IClaimsService claimsService)
        {
            _orderServiceTaskService = orderServiceTaskService;
            _claims = claimsService;
        }
        [HttpGet]
        public async Task<IActionResult> GetTasks()
        {
            try
            {
                var tasks = await _orderServiceTaskService.GetTasks();
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
        [HttpGet("GetDailyTasksForGarndener")]
        public async Task<IActionResult> GetDailyTasksForGarndener()
        {
            try
            {
                var tasks = await _orderServiceTaskService.GetDailyTasksForGarndener(_claims.GetCurrentUserId);
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
        [HttpPut("UpdateProgress")]
        [Authorize(Roles = "Gardener")]
        public async Task<IActionResult> UpdateDailyTasks([FromBody] OrderServiceTasksModels orderServiceTasksModels)
        {
            try
            {
                await _orderServiceTaskService.UpdateTaskProgess(orderServiceTasksModels);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok();
        }
    }
}
