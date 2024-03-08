using Application.Interfaces;
using Application.ViewModels.BonsaiViewModel;

using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BonsaiController : ControllerBase
    {
        private readonly IBonsaiService _bonsaiService;
        private readonly IFirebaseService _firebaseService;
        private readonly IClaimsService _claims;

        public BonsaiController(IBonsaiService bonsaiService,
            IFirebaseService firebaseService,
            IClaimsService claimsService)
        {
            _bonsaiService = bonsaiService;
            _firebaseService = firebaseService;
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
                    return BadRequest("Không tìm thấy");
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
                    return BadRequest("Không tìm thấy");
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
                if (products.Items.Count == 0)
                {
                    return NotFound("Không tìm thấy");
                }
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
                await _bonsaiService.AddAsync(productModel, _claims.GetIsAdmin);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok();
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
            return Ok();
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
            return Ok();
        }
    }
}
