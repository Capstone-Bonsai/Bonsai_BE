using Application.Interfaces;
using Application.Services;
using Application.ViewModels.BonsaiViewModel;
using Application.ViewModels.CareStepViewModels;
using Application.ViewModels.CategoryViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CareStepController : ControllerBase
    {
        private readonly ICareStepService _careStepService;
        private readonly IClaimsService _claims;

        public CareStepController(ICareStepService careStepService,
            IClaimsService claimsService)
        {
            _careStepService = careStepService;
            _claims = claimsService;
        }
        [HttpGet("{categoryId}")]
        public async Task<IActionResult> GetAll([FromRoute] Guid categoryId)
        {
            try
            {
                var careSteps = await _careStepService.GetCareStepsByCategoryId(categoryId);
                if (careSteps.Items.Count == 0)
                {
                    return BadRequest("Không tìm thấy");
                }
                else
                {
                    return Ok(careSteps);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Post([FromBody] CareStepModel careStepModel)
        {
            try
            {
                await _careStepService.AddCareSteps(careStepModel);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok();
        }
        [HttpPut("{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Put([FromRoute] Guid id, [FromBody] CareStepUpdateModel careStepUpdateModel)
        {
            try
            {
                await _careStepService.UpdateCareStep(id, careStepUpdateModel);
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
                await _careStepService.DeleteCareStep(id);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok();
        }
    }
}
