using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
    }
}
