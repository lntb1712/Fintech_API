using Controllers;
using DomainService.Interfaces.Wallet;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Model.RequestModel.Wallet;

namespace Fintech_API.Controllers.Wallet
{
    [Route("api/[controller]")]
    [ApiController]
    public class SharedWalletMemberController(IHttpContextAccessor httpContextAccessor,
        ISharedWalletMemberService sharedWalletMemberService,
        IValidator<SharedWalletMemberRequest> validator) : BaseController(httpContextAccessor)
    {
        private readonly ISharedWalletMemberService _sharedWalletMemberService = sharedWalletMemberService;
        private readonly IValidator<SharedWalletMemberRequest> _validator = validator;

        [HttpGet("shared-wallet-members")]
        public async Task<IActionResult> GetSharedWalletMembers() => Ok(await _sharedWalletMemberService.GetAllSharedWalletMembersAsync(currentUserId));

        [HttpGet("shared-wallet-members/{id}")]
        public async Task<IActionResult> GetSharedWalletMemberById(Guid id) => Ok(await _sharedWalletMemberService.GetSharedWalletMemberByIdAsync(currentUserId, id));

        [HttpPost("shared-wallet-members")]
        public async Task<IActionResult> CreateSharedWalletMember(SharedWalletMemberRequest request)
        {
            var validationResult = await _validator.ValidateAsync(request);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

            return Ok(await _sharedWalletMemberService.CreateSharedWalletMemberAsync(currentUserId, request));
        }

        [HttpPut("shared-wallet-members/{id}")]
        public async Task<IActionResult> UpdateSharedWalletMember(Guid id, SharedWalletMemberRequest request)
        {
            var validationResult = await _validator.ValidateAsync(request);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

            return Ok(await _sharedWalletMemberService.UpdateSharedWalletMemberAsync(currentUserId, id, request));
        }

        [HttpDelete("shared-wallet-members/{id}")]
        public async Task<IActionResult> DeleteSharedWalletMember(Guid id) => Ok(await _sharedWalletMemberService.DeleteSharedWalletMemberAsync(currentUserId, id));
    }
}
