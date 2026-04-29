using Controllers;
using DomainService.Interfaces.Wallet;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Model.RequestModel.Wallet;

namespace Fintech_API.Controllers.Wallet
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController(IHttpContextAccessor httpContextAccessor, ITransactionService transactionService,
        IValidator<TransactionRequest> validator) : BaseController(httpContextAccessor)
    {
        private readonly ITransactionService _transactionService = transactionService;
        private readonly IValidator<TransactionRequest> _validator = validator;

        [HttpGet("transactions")]
        public async Task<IActionResult> GetTransactions() => Ok(await _transactionService.GetAllTransactionsAsync(currentUserId));

        [HttpGet("transactions/{id}")]
        public async Task<IActionResult> GetTransactionById(Guid id) => Ok(await _transactionService.GetTransactionByIdAsync(currentUserId, id));

        [HttpPost("transactions")]
        public async Task<IActionResult> CreateTransaction(TransactionRequest request)
        {
            var validationResult = await _validator.ValidateAsync(request);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

            return Ok(await _transactionService.CreateTransactionAsync(currentUserId, request));
        }

        [HttpPut("transactions/{id}")]
        public async Task<IActionResult> UpdateTransaction(Guid id, TransactionRequest request)
        {
            var validationResult = await _validator.ValidateAsync(request);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

            return Ok(await _transactionService.UpdateTransactionAsync(currentUserId, id, request));
        }

        [HttpDelete("transactions/{id}")]
        public async Task<IActionResult> DeleteTransaction(Guid id) => Ok(await _transactionService.DeleteTransactionAsync(currentUserId, id));
    }
}
