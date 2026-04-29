using Model.RequestModel.Wallet;

namespace DomainService.Interfaces.Wallet
{
    public interface ITransferService
    {
        Task<object> CreateTransferAsync(Guid currentUserId, TransferRequest request);
        Task<object> GetTransferByIdAsync(Guid currentUserId, Guid transferId);
        Task<object> GetAllTransfersAsync(Guid currentUserId);
        Task<object> UpdateTransferAsync(Guid currentUserId, Guid transferId, TransferRequest request);
        Task<object> DeleteTransferAsync(Guid currentUserId, Guid transferId);
    }
}
