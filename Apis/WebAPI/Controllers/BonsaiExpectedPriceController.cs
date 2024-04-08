using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BonsaiExpectedPriceController : ControllerBase
    {
        private readonly IClaimsService _claims;
        private readonly IBonsaiExpectedPriceService _categoryExpectedPriceService;

        public BonsaiExpectedPriceController(IBonsaiExpectedPriceService categoryExpectedPriceService,
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
        [HttpPost]
        public async Task<IActionResult> PostAsync(IFormFile file)
        {
            try
            {
                await _categoryExpectedPriceService.CreateAsync(file);
                return Ok("Thêm bảng giá dịch vụ chăm sóc cây thành công.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
        [HttpPut]
        public async Task<IActionResult> PutAsync(IFormFile file)
        {
            try
            {
                await _categoryExpectedPriceService.UpdateAsync(file);
                return Ok("Cập nhật bảng giá dịch vụ chăm sóc cây từ file Excel thành công!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
