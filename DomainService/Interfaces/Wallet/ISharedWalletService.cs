using Model.RequestModel.Wallet;

namespace DomainService.Interfaces.Wallet
{
    public interface ISharedWalletService
    {
        Task<object> CreateSharedWalletAsync(Guid currentUserId, SharedWalletRequest request);
        Task<object> GetSharedWalletByIdAsync(Guid currentUserId, Guid sharedWalletId);
        Task<object> GetAllSharedWalletsAsync(Guid currentUserId);
        Task<object> UpdateSharedWalletAsync(Guid currentUserId, Guid sharedWalletId, SharedWalletRequest request);
        Task<object> DeleteSharedWalletAsync(Guid currentUserId, Guid sharedWalletId);
    }
}
