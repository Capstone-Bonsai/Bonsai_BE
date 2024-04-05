using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryExpectedPriceController : ControllerBase
    {
        private readonly IClaimsService _claims;
        private readonly ICategoryExpectedPriceService _categoryExpectedPriceService;

        public CategoryExpectedPriceController(ICategoryExpectedPriceService categoryExpectedPriceService,
            IClaimsService claimsService)
        {
            _claims = claimsService;
            _categoryExpectedPriceService = categoryExpectedPriceService;
        }
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var categories = await _categoryExpectedPriceService.Get();
                if (categories.Items.Count == 0)
                {
                    return BadRequest("Không tìm thấy");
                }
                else
                {
                    return Ok(categories);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("Price/{height}")]
        public async Task<IActionResult> GetPrice([FromRoute] float height)
        {
            try
            {
                var categories = _categoryExpectedPriceService.GetPrice(height);
                if (categories == 0)
                {
                    return BadRequest("Không tìm thấy");
                }
                else
                {
                    return Ok(categories);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
