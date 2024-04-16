using Application.Repositories;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
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
        [Authorize(Roles = "Manager")]
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
        [Authorize(Roles = "Manager")]
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

        [HttpGet("CalculateFee")]
        public async Task<IActionResult> CalculateFeeAsync(string destination,IList<Guid> listBonsaiId)
        {
            try
            {
                var newList = listBonsaiId.Distinct().ToList();
                var distance = await _deliveryFeeService.CalculateFee(destination, newList);
                return Ok(distance);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("test")]
        public async Task<IActionResult> Test(int distance)
        {
            try
            {
                
                var price = await _deliveryFeeService.TestCalcutale(distance);
                return Ok(price);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("Distance")]
        public async Task<IActionResult> DistanceAsync(string destination)
        {
            try
            {
                var distance = await _deliveryFeeService.GetDistanse(destination);
                return Ok(distance);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

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
        }

    }
}
