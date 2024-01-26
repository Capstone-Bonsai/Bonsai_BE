using Application.Interfaces;
using Application.ViewModels.TagViewModels;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TagController : ControllerBase
    {
        private readonly ITagService _tagService;
        private readonly IClaimsService _claims;

        public TagController(ITagService tagService,
            IClaimsService claimsService)
        {
            _tagService = tagService;
            _claims = claimsService;
        }
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var tags = await _tagService.GetTags();
                if (tags == null)
                {
                    return BadRequest("Không tìm thấy");
                }
                else
                {
                    return Ok(tags);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] TagModel tagModel)
        {
            try
            {
                await _tagService.AddTag(tagModel);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok();
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] Guid id, [FromBody] TagModel tagModel)
        {
            try
            {
                await _tagService.UpdateTag(id, tagModel);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok();
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            try
            {
                await _tagService.DeleteTag(id);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok();
        }
    }
}
