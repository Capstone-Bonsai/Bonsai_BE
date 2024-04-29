using Application.Interfaces;
using Application.Services;
using Application.ViewModels.BonsaiViewModel;
using Application.ViewModels.CustomerGardenViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerGardenController : ControllerBase
    {
        private readonly ICustomerGardenService _customerGardenService;
        private readonly IClaimsService _claims;

        public CustomerGardenController(ICustomerGardenService customerGardenService,
            IClaimsService claimsService)
        {
            _customerGardenService = customerGardenService;
            _claims = claimsService;
        }
        [HttpGet]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var customerGarden = await _customerGardenService.GetAllByCustomerId(_claims.GetCurrentUserId);
                if (customerGarden.Items.Count == 0)
                {
                    throw new Exception("Không tìm thấy");
                }
                return Ok(customerGarden);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("Pagination")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Get([FromQuery] int pageIndex, int pageSize)
        {
            try
            {
                var customerGarden = await _customerGardenService.GetByCustomerId(pageIndex, pageSize,_claims.GetCurrentUserId);
                return Ok(customerGarden);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Post([FromForm] CustomerGardenModel customerGardenModel)
        {
            try
            {
                await _customerGardenService.AddCustomerGarden(customerGardenModel, _claims.GetCurrentUserId);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("Manager/Pagination")]
        [Authorize(Roles = "Manager,Staff")]
        public async Task<IActionResult> GetForAdmin([FromQuery] int pageIndex, int pageSize)
        {
            try
            {
                var customerGarden = await _customerGardenService.GetPaginationForManager(pageIndex, pageSize);
                return Ok(customerGarden);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPut("{customerGardenId}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Put([FromRoute] Guid customerGardenId,[FromForm] CustomerGardenModel customerGardenModel)
        {
            try
            {
                await _customerGardenService.UpdateCustomerGarden(customerGardenId ,customerGardenModel, _claims.GetCurrentUserId);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            try
            {
                var bonsai = await _customerGardenService.GetById(id, _claims.GetIsCustomer, _claims.GetCurrentUserId);
                return Ok(bonsai);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
    }
}
