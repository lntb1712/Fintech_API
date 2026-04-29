using Controllers;
using DomainService.Interfaces.Wallet;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Model.RequestModel.Wallet;

namespace Fintech_API.Controllers.Wallet
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController(IHttpContextAccessor httpContextAccessor, ICategoryService categoryService,
        IValidator<CategoryRequest> validator) : BaseController(httpContextAccessor)
    {
        private readonly ICategoryService _categoryService = categoryService;
        private readonly IValidator<CategoryRequest> _validator = validator;

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories() => Ok(await _categoryService.GetAllCategoriesAsync(currentUserId));

        [HttpGet("categories/{id}")]
        public async Task<IActionResult> GetCategoryById(Guid id) => Ok(await _categoryService.GetCategoryByIdAsync(currentUserId, id));

        [HttpPost("categories")]
        public async Task<IActionResult> CreateCategory(CategoryRequest request)
        {
            var validationResult = await _validator.ValidateAsync(request);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

            return Ok(await _categoryService.CreateCategoryAsync(currentUserId, request));
        }

        [HttpPut("categories/{id}")]
        public async Task<IActionResult> UpdateCategory(Guid id, CategoryRequest request)
        {
            var validationResult = await _validator.ValidateAsync(request);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

            return Ok(await _categoryService.UpdateCategoryAsync(currentUserId, id, request));
        }

        [HttpDelete("categories/{id}")]
        public async Task<IActionResult> DeleteCategory(Guid id) => Ok(await _categoryService.DeleteCategoryAsync(currentUserId, id));
    }
}
