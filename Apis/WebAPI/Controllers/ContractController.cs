using Application.Interfaces;
using Application.Services.Momo;
using Application.Services;
using Application.ViewModels.ContractViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Domain.Enums;

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
        [Authorize]
        public async Task<IActionResult> Get([FromQuery] int pageIndex, int pageSize)
        {
            try
            {
                var contracts = await _contractService.GetContracts(pageIndex, pageSize, _claims.GetIsCustomer, _claims.GetCurrentUserId);
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
        [Authorize(Roles = "Staff")]
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
       
        [HttpGet("WorkingCalendar")]
        [Authorize(Roles = "Gardener")]
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
        [HttpGet("TodayProject")]
        [Authorize(Roles = "Gardener")]
        public async Task<IActionResult> GetTodayProject()
        {
            try
            {
                var contracts = await _contractService.GetTodayProject(_claims.GetCurrentUserId);
                return Ok(contracts);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("Gardener/{id}")]
        [Authorize(Roles = "Gardener")]
        public async Task<IActionResult> GetByIdForGardener(Guid id)
        {
            try
            {
                var contracts = await _contractService.GetContractByIdForGardener(id);
                return Ok(contracts);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("IpnHandler")]
        public async Task<IActionResult> IpnAsync([FromBody] MomoRedirect momo)
        {
            try
            {
                await _contractService.HandleIpnAsync(momo);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("Payment")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Payment(Guid ContractId)
        {
            try
            {
                Guid userId = _claims.GetCurrentUserId;
                
                var linkPayment = await _contractService.PaymentContract(ContractId, userId.ToString().ToLower() );
                if (linkPayment == null)
                    return BadRequest("Không tìm thấy");
                else
                    return Ok(linkPayment);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var contracts = await _contractService.GetContractById(id, _claims.GetIsCustomer, _claims.GetCurrentUserId);
                return Ok(contracts);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("Image/{contractId}")]
        [Authorize(Roles = "Staff,Manager")]
        public async Task<IActionResult> AddIMage([FromRoute] Guid contractId, [FromForm] ContractImageModel contractImageModel)
        {
            try
            {
                await _contractService.AddContractImage(contractId, contractImageModel);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [HttpPut("{contractId}")]
        [Authorize(Roles = "Manager,Staff")]
        [Authorize]
        public async Task<IActionResult> UpdateStatusAsync(Guid contractId, ContractStatus contractStatus)
        {
            try
            {
                await _contractService.UpdateContractStatus(contractId, contractStatus);
                return Ok("Cập nhật trạng thái hóa đơn thành công.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
