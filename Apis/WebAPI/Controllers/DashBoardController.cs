using Application.Interfaces;
using Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashBoardController : ControllerBase
    {
        private readonly IDashBoardService _dashBoardService;

        public DashBoardController(IDashBoardService dashBoardService)
        {
            _dashBoardService = dashBoardService;
        }
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var dashboard = await _dashBoardService.GetDashboardAsync();
                return BadRequest("Không tìm thấy!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("RevenueLineGraph")]
        public async Task<IActionResult> GetLineGraph()
        {
            try
            {
                var dashboard = await _dashBoardService.GetRevenueLineGraph();
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
                byte[] excelBytes = await _dashBoardService.GetExcel(); 
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "OrdersAndServiceOrders.xlsx");
            }
        }
        [HttpGet("Staff")]
        public async Task<IActionResult> GetForStaff()
        {
            try
            {
                var dashboard = await _dashBoardService.GetDashboardForStaffAsync();
                return Ok(dashboard);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
