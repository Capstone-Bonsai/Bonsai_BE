using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContractController : ControllerBase
    {
        private readonly IContractService _contractService;
        private readonly IClaimsService _claims;

        public ContractController(IContractService contractService,
            IClaimsService claimsService)
        {
            _contractService = contractService;
            _claims = claimsService;
        }
        [HttpGet("{contractId}")]
        public async Task<IActionResult> Get([FromRoute] Guid contractId)
        {
            try
            {
                var categories = await _contractService.GetTaskOfContract(contractId);
                if (categories.Count == 0)
                {
                    return BadRequest("Không tìm thấy");
                }
                else
                {
                    return Ok(categories);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
