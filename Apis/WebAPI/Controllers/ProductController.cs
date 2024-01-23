using Application.Interfaces;
using Application.ViewModels.UserViewModels;
using Domain.Entities;
using Infrastructures.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IClaimsService _claims;

        public ProductController(IProductService productService,
            IClaimsService claimsService)
        {
            _productService = productService;
            _claims = claimsService;
        }
        [HttpGet("Pagination")]
        public async Task<IActionResult> GetPagination([FromQuery] int pageIndex, int pageSize)
        {
            string userId = _claims.GetCurrentUserId.ToString().ToLower();
            try
            {
                var products = await _productService.GetProducts(pageIndex, pageSize);
                if (products == null)
                {
                    return BadRequest("Khong tim thay");
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
