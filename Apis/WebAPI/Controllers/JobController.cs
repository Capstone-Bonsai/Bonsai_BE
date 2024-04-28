using Application.Interfaces;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles ="Manager")]
    public class JobController : ControllerBase
    {
        private readonly IServiceOrderService _serviceOrderService;

        public JobController(IServiceOrderService serviceOrderService)
        {
            _serviceOrderService = serviceOrderService;
        }
        [HttpGet]
        [Route("/CanceledOverdueServiceOrderJob")]
        public async Task<IActionResult> CanceledOverdueServiceOrderJob()
        {
            RecurringJob.RemoveIfExists("StartContract");
            RecurringJob.AddOrUpdate("StartContract", () => _serviceOrderService.CancelOverdueServiceOrder(), "0 30 0 * * ?", TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
            return Ok();
        }

    }
    
}
