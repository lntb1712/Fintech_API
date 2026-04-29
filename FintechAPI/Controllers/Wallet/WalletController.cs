using Controllers;
using DomainService.Interfaces.Account;
using DomainService.Interfaces.Wallet;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Model.RequestModel.PermissionManagement;
using Model.RequestModel.Wallet;

namespace Fintech_API.Controllers.Wallet
{
    [Route("api/[controller]")]
    [ApiController]
    public class WalletController(IHttpContextAccessor httpContextAccessor, IWalletService walletService,
                                IValidator<WalletRequest> validator) : BaseController(httpContextAccessor)
    {
        private readonly IWalletService _walletService = walletService;
        private readonly IValidator<WalletRequest> _validator = validator;

        [HttpGet("wallets")]
        public async Task<IActionResult> GetWallets()
        {
            var result = await _walletService.GetAllWalletsAsync(currentUserId);
            return Ok(result);
        }

        [HttpGet("wallets/{id}")]
        public async Task<IActionResult> GetWalletById(Guid id)
        {
            var result = await _walletService.GetWalletByIdAsync(currentUserId, id);
            return Ok(result);
        }

        [HttpPost("wallets")]
        public async Task<IActionResult> CreateWallet(WalletRequest request)
        {
            var validationResult = await _validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
            }
            var result = await _walletService.CreateWalletAsync(currentUserId, request);
            return Ok(result);
        }

        [HttpPut("wallets/{id}")]
        public async Task<IActionResult> UpdateWallet(Guid id, WalletRequest request)
        {
            var validationResult = await _validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
            }
            var result = await _walletService.UpdateWalletAsync(currentUserId, id, request);
            return Ok(result);
        }

        [HttpDelete("wallets/{id}")]
        public async Task<IActionResult> DeleteWallet(Guid id)
        {
            var result = await _walletService.DeleteWalletAsync(currentUserId, id);
            return Ok(result);
        }
    }
}
