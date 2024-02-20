using Application.Interfaces;
using Application.Services;
using Application.ViewModels.AnnualWorkingDayModel;
using Application.ViewModels.ProductViewModels;
using Microsoft.AspNetCore.Authorization;
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
        [HttpGet]
        public async Task<IActionResult> GetAnnualWorkingDays()
        {
            try
            {
                var annualWorkingDays = await _annualWorkingDayService.GetAnnualWorkingDays();
                if (annualWorkingDays.Items.Count == 0)
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
        [HttpPost("{serviceOrderId}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Post([FromRoute] Guid serviceOrderId, [FromBody] GardernerListModel gardernerListModel)
        {
            try
            {
                await _annualWorkingDayService.AddWorkingday(serviceOrderId, gardernerListModel);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok();
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
