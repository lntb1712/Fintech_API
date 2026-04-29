using Controllers;
using DomainService.Interfaces.Wallet;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Model.RequestModel.Wallet;

namespace Fintech_API.Controllers.Wallet
{
    [Route("api/[controller]")]
    [ApiController]
    public class GoalController(IHttpContextAccessor httpContextAccessor, IGoalService goalService,
        IValidator<GoalRequest> validator) : BaseController(httpContextAccessor)
    {
        private readonly IGoalService _goalService = goalService;
        private readonly IValidator<GoalRequest> _validator = validator;

        [HttpGet("goals")]
        public async Task<IActionResult> GetGoals() => Ok(await _goalService.GetAllGoalsAsync(currentUserId));

        [HttpGet("goals/{id}")]
        public async Task<IActionResult> GetGoalById(Guid id) => Ok(await _goalService.GetGoalByIdAsync(currentUserId, id));

        [HttpPost("goals")]
        public async Task<IActionResult> CreateGoal(GoalRequest request)
        {
            var validationResult = await _validator.ValidateAsync(request);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

            return Ok(await _goalService.CreateGoalAsync(currentUserId, request));
        }

        [HttpPut("goals/{id}")]
        public async Task<IActionResult> UpdateGoal(Guid id, GoalRequest request)
        {
            var validationResult = await _validator.ValidateAsync(request);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

            return Ok(await _goalService.UpdateGoalAsync(currentUserId, id, request));
        }

        [HttpDelete("goals/{id}")]
        public async Task<IActionResult> DeleteGoal(Guid id) => Ok(await _goalService.DeleteGoalAsync(currentUserId, id));
    }
}
