using Application.Interfaces;
using Application.Services;
using Application.ViewModels.BonsaiViewModel;
using Application.ViewModels.CustomerBonsaiViewModels;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerBonsaiController : ControllerBase
    {
        private readonly ICustomerBonsaiService _customerBonsaiService;
        private readonly IClaimsService _claims;

        public CustomerBonsaiController(ICustomerBonsaiService customerBonsaiService,
            IClaimsService claimsService)
        {
            _customerBonsaiService = customerBonsaiService;
            _claims = claimsService;
        }
        [HttpPost("BoughtBonsai")]
        public async Task<IActionResult> AddBonsaiForCustomer([FromBody] CustomerBonsaiModel customerBonsaiModel)
        {
            try
            {
                 await _customerBonsaiService.AddBonsaiForCustomer(customerBonsaiModel, _claims.GetCurrentUserId);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
        [HttpPost("Customer/{gardenId}")]
        public async Task<IActionResult> Post([FromRoute] Guid gardenId, [FromForm] BonsaiModelForCustomer bonsaiModelForCustomer)
        {
            try
            {
                await _customerBonsaiService.CreateBonsai(gardenId, bonsaiModelForCustomer);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok();
        }
        [HttpGet("CustomerGarden/{gardenId}")]
        [Authorize(Roles = "Manager,Customer,Staff")]
        public async Task<IActionResult> Post([FromRoute] Guid gardenId)
        {
            try
            {
                var bonsai = await _customerBonsaiService.GetBonsaiOfGarden(gardenId);
                return Ok(bonsai);
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
                var bonsai = await _customerBonsaiService.GetCustomerBonsaiById(id, _claims.GetCurrentUserId, _claims.GetIsCustomer);
                return Ok(bonsai);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
        [HttpPut("MoveBonsai")]
        public async Task<IActionResult> MoveBonsai([FromRoute] Guid customerBonsaiId, Guid customerGardenId)
        {
            try
            {
                await _customerBonsaiService.MoveBonsai(_claims.GetCurrentUserId, customerBonsaiId, customerGardenId);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPut("{customerBonsaiId}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Put([FromRoute] Guid customerBonsaiId, [FromForm] BonsaiModel bonsaiModel)
        {
            try
            {
                await _customerBonsaiService.Update(customerBonsaiId, bonsaiModel);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok("Cập nhật thành công!");
        }
         [HttpGet("Pagination")]
        public async Task<IActionResult> GetPagination([FromQuery] int pageIndex, int pageSize)
        {
            try
            {
                var customerBonsai = await _customerBonsaiService.GetBonsaiOfCustomer(_claims.GetCurrentUserId, pageIndex, pageSize);
                if (customerBonsai.Items.Count == 0)
                {
                    return BadRequest("Không tìm thấy!");
                }
                else
                {
                    return Ok(customerBonsai);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        public async Task<IActionResult> Post([FromForm] BonsaiModelForCustomer bonsaiModelForCustomer)
        {
            try
            {
                await _customerBonsaiService.CreateBonsaiWithNewGarden(bonsaiModelForCustomer);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok();
        }
    }
}
