using Controllers;
using DomainService.Interfaces.Wallet;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Model.RequestModel.Wallet;

namespace Fintech_API.Controllers.Wallet
{
    [Route("api/[controller]")]
    [ApiController]
    public class SharedWalletController(IHttpContextAccessor httpContextAccessor, ISharedWalletService sharedWalletService,
        IValidator<SharedWalletRequest> validator) : BaseController(httpContextAccessor)
    {
        private readonly ISharedWalletService _sharedWalletService = sharedWalletService;
        private readonly IValidator<SharedWalletRequest> _validator = validator;

        [HttpGet("shared-wallets")]
        public async Task<IActionResult> GetSharedWallets() => Ok(await _sharedWalletService.GetAllSharedWalletsAsync(currentUserId));

        [HttpGet("shared-wallets/{id}")]
        public async Task<IActionResult> GetSharedWalletById(Guid id) => Ok(await _sharedWalletService.GetSharedWalletByIdAsync(currentUserId, id));

        [HttpPost("shared-wallets")]
        public async Task<IActionResult> CreateSharedWallet(SharedWalletRequest request)
        {
            var validationResult = await _validator.ValidateAsync(request);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

            return Ok(await _sharedWalletService.CreateSharedWalletAsync(currentUserId, request));
        }

        [HttpPut("shared-wallets/{id}")]
        public async Task<IActionResult> UpdateSharedWallet(Guid id, SharedWalletRequest request)
        {
            var validationResult = await _validator.ValidateAsync(request);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

            return Ok(await _sharedWalletService.UpdateSharedWalletAsync(currentUserId, id, request));
        }

        [HttpDelete("shared-wallets/{id}")]
        public async Task<IActionResult> DeleteSharedWallet(Guid id) => Ok(await _sharedWalletService.DeleteSharedWalletAsync(currentUserId, id));
    }
}
