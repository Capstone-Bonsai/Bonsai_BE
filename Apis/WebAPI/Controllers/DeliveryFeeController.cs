using Application.Repositories;
using Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeliveryFeeController : ControllerBase
    {
        private readonly IDeliveryFeeService _deliveryFeeService;

        public DeliveryFeeController(IDeliveryFeeService deliveryFeeService)
        {
            _deliveryFeeService = deliveryFeeService;
        }
        [HttpPost]
        public async Task<IActionResult> PostAsync(IFormFile file)
        {
            try
            {
                
                await _deliveryFeeService.CreateAsync(file);
                return Ok("Thêm bảng giá phí giao hàng từ file Excel thành công!");
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
                await _deliveryFeeService.UpdateAsync(file);
                return Ok("Cập nhật bảng giá phí giao hàng từ file Excel thành công!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> CalculateFeeAsync(string destination,double price)
        {
            try
            {
                var distance = await _deliveryFeeService.CalculateFee(destination,  price);
                return Ok(distance);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        /*
                [HttpGet]
                public async Task<IActionResult> GetAsync()
                {
                    try
                    {

                        var list = await _deliveryFeeService.GetAllAsync();
                        return Ok(list);
                    }
                    catch (Exception ex)
                    {
                        return BadRequest(ex.Message);
                    }
                }*/

    }
}
