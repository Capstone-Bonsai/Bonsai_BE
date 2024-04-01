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
        [Authorize(Roles = "Manager,Customer")]
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
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            try
            {
                var bonsai = await _customerBonsaiService.GetCustomerBonsaiById(id);
                return Ok(bonsai);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
    }
}
