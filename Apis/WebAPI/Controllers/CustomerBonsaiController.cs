using Application.Interfaces;
using Application.ViewModels.CustomerBonsaiViewModels;
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
        [HttpPost]
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
    }
}
