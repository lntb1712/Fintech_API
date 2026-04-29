using Controllers;
using DomainService.Interfaces.Wallet;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Model.RequestModel.Wallet;

namespace Fintech_API.Controllers.Wallet
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransferController(IHttpContextAccessor httpContextAccessor, ITransferService transferService,
        IValidator<TransferRequest> validator) : BaseController(httpContextAccessor)
    {
        private readonly ITransferService _transferService = transferService;
        private readonly IValidator<TransferRequest> _validator = validator;

        [HttpGet("transfers")]
        public async Task<IActionResult> GetTransfers() => Ok(await _transferService.GetAllTransfersAsync(currentUserId));

        [HttpGet("transfers/{id}")]
        public async Task<IActionResult> GetTransferById(Guid id) => Ok(await _transferService.GetTransferByIdAsync(currentUserId, id));

        [HttpPost("transfers")]
        public async Task<IActionResult> CreateTransfer(TransferRequest request)
        {
            var validationResult = await _validator.ValidateAsync(request);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

            return Ok(await _transferService.CreateTransferAsync(currentUserId, request));
        }

        [HttpPut("transfers/{id}")]
        public async Task<IActionResult> UpdateTransfer(Guid id, TransferRequest request)
        {
            var validationResult = await _validator.ValidateAsync(request);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

            return Ok(await _transferService.UpdateTransferAsync(currentUserId, id, request));
        }

        [HttpDelete("transfers/{id}")]
        public async Task<IActionResult> DeleteTransfer(Guid id) => Ok(await _transferService.DeleteTransferAsync(currentUserId, id));
    }
}
