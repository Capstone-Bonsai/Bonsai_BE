using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnnualWorkingDayController : ControllerBase
    {
        private readonly IAnnualWorkingDayService _annualWorkingDayService;
        private readonly IClaimsService _claims;

        public AnnualWorkingDayController(IAnnualWorkingDayService annualWorkingDayService,
            IClaimsService claimsService)
        {
            _annualWorkingDayService = annualWorkingDayService;
            _claims = claimsService;
        }
        [HttpGet("{month}/{year}")]
        public async Task<IActionResult> Get(int month, int year)
        {
            try
            {
                var annualWorkingDays = await _annualWorkingDayService.GetWorkingCalencar(_claims.GetCurrentUserId, month, year);
                if (annualWorkingDays.Count == 0)
                {
                    return BadRequest("Không tìm thấy");
                }
                else
                {
                    return Ok(annualWorkingDays);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
