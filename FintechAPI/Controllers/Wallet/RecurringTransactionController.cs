using Controllers;
using DomainService.Interfaces.Wallet;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Model.RequestModel.Wallet;

namespace Fintech_API.Controllers.Wallet
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecurringTransactionController(IHttpContextAccessor httpContextAccessor,
        IRecurringTransactionService recurringTransactionService,
        IValidator<RecurringTransactionRequest> validator) : BaseController(httpContextAccessor)
    {
        private readonly IRecurringTransactionService _recurringTransactionService = recurringTransactionService;
        private readonly IValidator<RecurringTransactionRequest> _validator = validator;

        [HttpGet("recurring-transactions")]
        public async Task<IActionResult> GetRecurringTransactions() => Ok(await _recurringTransactionService.GetAllRecurringTransactionsAsync(currentUserId));

        [HttpGet("recurring-transactions/{id}")]
        public async Task<IActionResult> GetRecurringTransactionById(Guid id) => Ok(await _recurringTransactionService.GetRecurringTransactionByIdAsync(currentUserId, id));

        [HttpPost("recurring-transactions")]
        public async Task<IActionResult> CreateRecurringTransaction(RecurringTransactionRequest request)
        {
            var validationResult = await _validator.ValidateAsync(request);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

            return Ok(await _recurringTransactionService.CreateRecurringTransactionAsync(currentUserId, request));
        }

        [HttpPut("recurring-transactions/{id}")]
        public async Task<IActionResult> UpdateRecurringTransaction(Guid id, RecurringTransactionRequest request)
        {
            var validationResult = await _validator.ValidateAsync(request);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

            return Ok(await _recurringTransactionService.UpdateRecurringTransactionAsync(currentUserId, id, request));
        }

        [HttpDelete("recurring-transactions/{id}")]
        public async Task<IActionResult> DeleteRecurringTransaction(Guid id) => Ok(await _recurringTransactionService.DeleteRecurringTransactionAsync(currentUserId, id));
    }
}
