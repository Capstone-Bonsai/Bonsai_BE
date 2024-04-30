using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RevenueController : ControllerBase
    {
        private readonly IRevenueService _revenueService;

        public RevenueController(IRevenueService revenueService)
        {
            _revenueService = revenueService;
        }
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var dashboard = await _revenueService.GetRevenueAsync();
                return Ok(dashboard);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("download")]
        public async Task<IActionResult> DownloadExcel()
        {
            using (var package = new ExcelPackage())
            {
                byte[] excelBytes = await _revenueService.GetExcel();
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "OrdersAndServiceOrders.xlsx");
            }
        }
        [HttpGet("Order")]
        public async Task<IActionResult> GetOrder()
        {
            try
            {
                var dashboard = await _revenueService.GetOrdersAsync();
                return Ok(dashboard);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("ServiceOrder")]
        public async Task<IActionResult> GetServiceOrder()
        {
            try
            {
                var dashboard = await _revenueService.GetServiceOrdersAsync();
                return Ok(dashboard);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
