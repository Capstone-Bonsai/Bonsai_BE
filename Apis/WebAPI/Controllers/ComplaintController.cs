using Application.Interfaces;
using Application.Services;
using Application.ViewModels.ComplaintModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Services;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ComplaintController : ControllerBase
    {
        private readonly IComplaintService _complaintService;
        private readonly IClaimsService _claimsService;

        public ComplaintController(IComplaintService complaintService,IClaimsService claimsService)
        {
            _complaintService = complaintService;
            _claimsService= claimsService;
        }
        [Authorize(Roles ="Customer")]
        [HttpPost]
        public async Task<IActionResult> PostAsync([FromForm] ComplaintModel model)
        {
            if(model.ListImage == null || model.ListImage.Count == 0)
            {
                return BadRequest("Vui lòng thêm hình ảnh.");
            }
            try
            {
                var userId = _claimsService.GetCurrentUserId.ToString().ToLower().Trim();
                await _complaintService.CreateComplaint(userId, model);
                return Ok("Tạo đơn phản ánh thành công.");   
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize(Roles = "Manager,Staff")]
        [HttpPut]
        public async Task<IActionResult> PutAsync([FromBody] ComplaintUpdateModel model)
        {
            try
            {
                
                await _complaintService.ReplyComplaint( model);
                return Ok("Cập nhật trạng thái kiếu nại thành công thành công.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize(Roles = "Manager,Staff")]
        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            try
            {
                var list = await _complaintService.GetList();
                return Ok(list);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
