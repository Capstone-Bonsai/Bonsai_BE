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
        [HttpGet("{contractId}")]
        public async Task<IActionResult> GetGardenerOfContract([FromQuery] int pageIndex, int pageSize,[FromRoute] Guid contractId)
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
        [HttpPut("ChangeGardener/{contractId}")]
        public async Task<IActionResult> Post([FromRoute] Guid contractId,[FromBody] ChangeGardenerViewModel changeGardenerViewModel)
        {
            try
            {
                await _contractGardenerService.ChangeContractGardener(contractId, changeGardenerViewModel);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ContractGardenerModel contractGardenerModel)
        {
            try
            {
                await _contractGardenerService.AddContractGardener(contractGardenerModel);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
