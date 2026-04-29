using Model.RequestModel.Wallet;

namespace DomainService.Interfaces.Wallet
{
    public interface ISharedWalletMemberService
    {
        Task<object> CreateSharedWalletMemberAsync(Guid currentUserId, SharedWalletMemberRequest request);
        Task<object> GetSharedWalletMemberByIdAsync(Guid currentUserId, Guid sharedWalletMemberId);
        Task<object> GetAllSharedWalletMembersAsync(Guid currentUserId);
        Task<object> UpdateSharedWalletMemberAsync(Guid currentUserId, Guid sharedWalletMemberId, SharedWalletMemberRequest request);
        Task<object> DeleteSharedWalletMemberAsync(Guid currentUserId, Guid sharedWalletMemberId);
    }
}
