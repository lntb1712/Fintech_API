using Controllers;
using DomainService.Interfaces.Wallet;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Model.RequestModel.Wallet;

namespace Fintech_API.Controllers.Wallet
{
    [Route("api/[controller]")]
    [ApiController]
    public class BudgetController(IHttpContextAccessor httpContextAccessor, IBudgetService budgetService,
        IValidator<BudgetRequest> validator) : BaseController(httpContextAccessor)
    {
        private readonly IBudgetService _budgetService = budgetService;
        private readonly IValidator<BudgetRequest> _validator = validator;

        [HttpGet("budgets")]
        public async Task<IActionResult> GetBudgets() => Ok(await _budgetService.GetAllBudgetsAsync(currentUserId));

        [HttpGet("budgets/{id}")]
        public async Task<IActionResult> GetBudgetById(Guid id) => Ok(await _budgetService.GetBudgetByIdAsync(currentUserId, id));

        [HttpPost("budgets")]
        public async Task<IActionResult> CreateBudget(BudgetRequest request)
        {
            var validationResult = await _validator.ValidateAsync(request);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

            return Ok(await _budgetService.CreateBudgetAsync(currentUserId, request));
        }

        [HttpPut("budgets/{id}")]
        public async Task<IActionResult> UpdateBudget(Guid id, BudgetRequest request)
        {
            var validationResult = await _validator.ValidateAsync(request);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

            return Ok(await _budgetService.UpdateBudgetAsync(currentUserId, id, request));
        }

        [HttpDelete("budgets/{id}")]
        public async Task<IActionResult> DeleteBudget(Guid id) => Ok(await _budgetService.DeleteBudgetAsync(currentUserId, id));
    }
}
