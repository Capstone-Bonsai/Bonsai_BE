﻿using Application.Interfaces;
using Application.Services;
using Application.ViewModels.BonsaiViewModel;
using Application.ViewModels.CustomerGardenViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerGardenController : ControllerBase
    {
        private readonly ICustomerGardenService _customerGardenService;
        private readonly IClaimsService _claims;

        public CustomerGardenController(ICustomerGardenService customerGardenService,
            IClaimsService claimsService)
        {
            _customerGardenService = customerGardenService;
            _claims = claimsService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var customerGarden = await _customerGardenService.Get();
                if (customerGarden.Items.Count == 0)
                {
                    throw new Exception("Không tìm thấy");
                }
                return Ok(customerGarden);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
        [HttpGet("Customer")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Get()
        {
            try
            {
               var customerGarden = await _customerGardenService.GetByCustomerId(_claims.GetCurrentUserId);
                if (customerGarden.Items.Count == 0)
                {
                    throw new Exception("Không tìm thấy");
                }
                return Ok(customerGarden);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Post([FromForm] CustomerGardenModel customerGardenModel)
        {
            try
            {
                await _customerGardenService.AddCustomerGarden(customerGardenModel, _claims.GetCurrentUserId);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}