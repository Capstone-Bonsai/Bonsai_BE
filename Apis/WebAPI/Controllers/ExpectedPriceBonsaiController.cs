using Application.Interfaces;
using Application.Repositories;
using Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExpectedPriceBonsaiController : ControllerBase
    {
        private readonly ICategoryExpectedPriceService _categoryExpectedPriceService;

        public ExpectedPriceBonsaiController(ICategoryExpectedPriceService categoryExpectedPriceService)
        {
            _categoryExpectedPriceService = categoryExpectedPriceService;
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
