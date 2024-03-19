﻿using Application.Interfaces;
using Application.ViewModels.TaskViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly ITaskService _taskService;
        private readonly IClaimsService _claims;

        public TaskController(ITaskService taskService,
            IClaimsService claimsService)
        {
            _taskService = taskService;
            _claims = claimsService;
        }
        [HttpGet("{contractId}")]
        public async Task<IActionResult> Get(Guid contractId)
        {
            try
            {
                var taskViewModel = await _taskService.GetTasksOfContract(contractId);
                if (taskViewModel == null)
                {
                    return BadRequest("Không tìm thấy");
                }
                else
                {
                    return Ok(taskViewModel);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("Progress")]
        public async Task<IActionResult> Post(TaskModel taskModel)
        {
            try
            {
                await _taskService.UpdateProgress(taskModel);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
