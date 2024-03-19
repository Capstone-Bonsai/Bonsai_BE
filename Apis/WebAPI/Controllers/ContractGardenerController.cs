using Application.Interfaces;
using Application.Services;
using Application.ViewModels.ContractViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContractGardenerController : ControllerBase
    {
        private readonly IContractGardenerService _contractGardenerService;
        private readonly IClaimsService _claims;

        public ContractGardenerController(IContractGardenerService contractGardenerService,
            IClaimsService claimsService)
        {
            _contractGardenerService = contractGardenerService;
            _claims = claimsService;
        }
        [HttpGet("GardenerOfContract")]
        public async Task<IActionResult> GetGardenerOfContract(int pageIndex, int pageSize, Guid contractId)
        {
            try
            {
                var users = await _contractGardenerService.GetGardenerOfContract(pageIndex, pageSize, contractId);
                return Ok(users);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpDelete]
        public async Task<IActionResult> Post([FromBody] Guid contractId, Guid gardenerId)
        {
            try
            {
                await _contractGardenerService.DeleteContractGardener(contractId, gardenerId);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
