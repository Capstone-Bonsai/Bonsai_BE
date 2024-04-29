﻿using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly IClaimsService _claims;

        public NotificationController(INotificationService notificationService,
            IClaimsService claimsService)
        {
            _notificationService = notificationService;
            _claims = claimsService;
        }
        [HttpGet("Pagination")]
        [Authorize]  
        public async Task<IActionResult> GetPagination([FromQuery] int pageIndex, int pageSize)
        {
            try
            {
                var notifications = await _notificationService.GetNotification(_claims.GetCurrentUserId,pageIndex, pageSize);
                return Ok(notifications);   
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("Pagination")]
        [Authorize]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            try
            {
                var notifications = await _notificationService.GetNotificationById(_claims.GetCurrentUserId, id);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
