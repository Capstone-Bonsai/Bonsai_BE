using Application.Interfaces;
using Application.ViewModels.ContractViewModels;
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
        [HttpGet("Pagination")]
        public async Task<IActionResult> Get([FromRoute] int pageIndex, int pageSize)
        {
            try
            {
                var contracts = await _contractService.GetContracts(pageIndex, pageSize);
                if (contracts.Items.Count == 0)
                {
                    return BadRequest("Không tìm thấy");
                }
                else
                {
                    return Ok(contracts);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ContractModel contractModel)
        {
            try
            {
                await _contractService.CreateContract(contractModel);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("ContractGardener")]
        public async Task<IActionResult> Post([FromBody] ContractGardenerModel contractGardenerModel)
        {
            try
            {
                await _contractService.AddContractGardener(contractGardenerModel);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("WorkingCalendar")]
        public async Task<IActionResult> GetWorkingCalendar(int month, int year)
        {
            try
            {
                var contracts = await _contractService.GetWorkingCalendar(month, year, _claims.GetCurrentUserId);
                return Ok(contracts);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var contracts = await _contractService.GetContractById(id);
                return Ok(contracts);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
