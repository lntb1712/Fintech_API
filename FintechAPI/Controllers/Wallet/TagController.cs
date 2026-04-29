using Controllers;
using DomainService.Interfaces.Wallet;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Model.RequestModel.Wallet;

namespace Fintech_API.Controllers.Wallet
{
    [Route("api/[controller]")]
    [ApiController]
    public class TagController(IHttpContextAccessor httpContextAccessor, ITagService tagService,
        IValidator<TagRequest> validator) : BaseController(httpContextAccessor)
    {
        private readonly ITagService _tagService = tagService;
        private readonly IValidator<TagRequest> _validator = validator;

        [HttpGet("tags")]
        public async Task<IActionResult> GetTags() => Ok(await _tagService.GetAllTagsAsync(currentUserId));

        [HttpGet("tags/{id}")]
        public async Task<IActionResult> GetTagById(Guid id) => Ok(await _tagService.GetTagByIdAsync(currentUserId, id));

        [HttpPost("tags")]
        public async Task<IActionResult> CreateTag(TagRequest request)
        {
            var validationResult = await _validator.ValidateAsync(request);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

            return Ok(await _tagService.CreateTagAsync(currentUserId, request));
        }

        [HttpPut("tags/{id}")]
        public async Task<IActionResult> UpdateTag(Guid id, TagRequest request)
        {
            var validationResult = await _validator.ValidateAsync(request);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

            return Ok(await _tagService.UpdateTagAsync(currentUserId, id, request));
        }

        [HttpDelete("tags/{id}")]
        public async Task<IActionResult> DeleteTag(Guid id) => Ok(await _tagService.DeleteTagAsync(currentUserId, id));
    }
}
