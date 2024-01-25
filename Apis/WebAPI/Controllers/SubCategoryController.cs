﻿using Application.Interfaces;
using Application.ViewModels.CategoryViewModels;
using Application.ViewModels.SubCategoryViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubCategoryController : ControllerBase
    {
        private readonly ISubCategoryService _subCategoryService;
        private readonly IClaimsService _claims;

        public SubCategoryController(ISubCategoryService subCategoryService,
            IClaimsService claimsService)
        {
            _subCategoryService = subCategoryService;
            _claims = claimsService;
        }
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var subCategories = await _subCategoryService.GetSubCategories();
                if (subCategories == null)
                {
                    return BadRequest("Không tìm thấy");
                }
                else
                {
                    return Ok(subCategories);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] SubCategoryModel subCategoryModel)
        {
            try
            {
                await _subCategoryService.AddSubCategory(subCategoryModel);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return NoContent();
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] Guid id, [FromBody] SubCategoryModel subCategoryModel)
        {
            try
            {
                await _subCategoryService.UpdateSubCategory(id, subCategoryModel);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return NoContent();
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            try
            {
                await _subCategoryService.DeleteSubCategory(id);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return NoContent();
        }
    }
}