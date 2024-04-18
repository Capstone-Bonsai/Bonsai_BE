using Application.Hubs;
using Application.Interfaces;
using Application.ViewModels.BonsaiViewModel;
using Application.ViewModels.MessageViewModels;
using Domain.Entities;
using Firebase.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BonsaiController : ControllerBase
    {
        private readonly IBonsaiService _bonsaiService;
        private readonly IClaimsService _claims;

        public BonsaiController(IBonsaiService bonsaiService,
            IClaimsService claimsService)
        {
            _bonsaiService = bonsaiService;
            _claims = claimsService;
        }
        [HttpGet("Pagination")]
        public async Task<IActionResult> GetPagination([FromQuery] int pageIndex, int pageSize)
        {
            try
            {
                var products = await _bonsaiService.GetPagination(pageIndex, pageSize, _claims.GetIsAdmin);
                if (products.Items.Count == 0)
                {
                    return BadRequest("Không tìm thấy!");
                }
                else
                {
                    return Ok(products);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var products = await _bonsaiService.GetAll(_claims.GetIsAdmin);
                if (products.Items.Count == 0)
                {
                    return BadRequest("Không tìm thấy!");
                }
                else
                {
                    return Ok(products);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("Filter")]
        public async Task<IActionResult> Post([FromQuery] int pageIndex, int pageSize, [FromBody] FilterBonsaiModel filterBonsaiModel)
        {
            try
            {
                var products = await _bonsaiService.GetByFilter(pageIndex, pageSize, filterBonsaiModel, _claims.GetIsAdmin);
                return Ok(products);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Post([FromForm] BonsaiModel productModel)
        {
            try
            {
                await _bonsaiService.AddAsync(productModel);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok("Tạo bonsai thành công!");
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> Get([FromRoute] Guid id)
        {
            var product = await _bonsaiService.GetById(id, _claims.GetIsAdmin);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }
        [HttpPut("{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Put([FromRoute] Guid id, [FromForm] BonsaiModel productModel)
        {
            try
            {
                await _bonsaiService.Update(id, productModel);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok("Cập nhật thành công!");
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            try
            {
                await _bonsaiService.Delete(id);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok("Xóa thành công!");
        }
        [HttpGet("BoughtBonsai")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetBoughtBonsai()
        {
            try
            {
                var products = await _bonsaiService.GetBoughtBonsai(_claims.GetCurrentUserId);
                if (products.Items.Count == 0)
                {
                    return BadRequest("Không tìm thấy!");
                }
                else
                {
                    return Ok(products);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("Category")]
        public async Task<IActionResult> GetByCategory([FromQuery] int pageIndex, int pageSize, Guid categoryId)
        {
            try
            {
                var products = await _bonsaiService.GetByCategory(pageIndex, pageSize, categoryId);
                if (products.Items.Count == 0)
                {
                    return BadRequest("Không tìm thấy!");
                }
                else
                {
                    return Ok(products);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPut("Disable")]
        public async Task<IActionResult> DisableBonsai([FromQuery] Guid bonsaiId)
        {
            try
            {
                await _bonsaiService.DisableBonsai(bonsaiId);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("CurrentCart")]
        public async Task<IActionResult> GetCurrentCart([FromBody] List<Guid> bonsaiId)
        {
            try
            {
                var products = await _bonsaiService.getCurrentCart(bonsaiId);
                if (products.Count == 0)
                {
                    return BadRequest("Không tìm thấy!");
                }
                else
                {
                    return Ok(products);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
