using Controllers;
using DomainService.Interfaces.Wallet;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Model.RequestModel.Wallet;

namespace Fintech_API.Controllers.Wallet
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionTagController(IHttpContextAccessor httpContextAccessor,
        ITransactionTagService transactionTagService,
        IValidator<TransactionTagRequest> validator) : BaseController(httpContextAccessor)
    {
        private readonly ITransactionTagService _transactionTagService = transactionTagService;
        private readonly IValidator<TransactionTagRequest> _validator = validator;

        [HttpGet("transaction-tags")]
        public async Task<IActionResult> GetTransactionTags() => Ok(await _transactionTagService.GetAllTransactionTagsAsync(currentUserId));

        [HttpGet("transaction-tags/{id}")]
        public async Task<IActionResult> GetTransactionTagById(Guid id) => Ok(await _transactionTagService.GetTransactionTagByIdAsync(currentUserId, id));

        [HttpPost("transaction-tags")]
        public async Task<IActionResult> CreateTransactionTag(TransactionTagRequest request)
        {
            var validationResult = await _validator.ValidateAsync(request);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

            return Ok(await _transactionTagService.CreateTransactionTagAsync(currentUserId, request));
        }

        [HttpPut("transaction-tags/{id}")]
        public async Task<IActionResult> UpdateTransactionTag(Guid id, TransactionTagRequest request)
        {
            var validationResult = await _validator.ValidateAsync(request);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

            return Ok(await _transactionTagService.UpdateTransactionTagAsync(currentUserId, id, request));
        }

        [HttpDelete("transaction-tags/{id}")]
        public async Task<IActionResult> DeleteTransactionTag(Guid id) => Ok(await _transactionTagService.DeleteTransactionTagAsync(currentUserId, id));
    }
}
