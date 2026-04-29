using Model.RequestModel.Wallet;

namespace DomainService.Interfaces.Wallet
{
    public interface ITransactionTagService
    {
        Task<object> CreateTransactionTagAsync(Guid currentUserId, TransactionTagRequest request);
        Task<object> GetTransactionTagByIdAsync(Guid currentUserId, Guid transactionTagId);
        Task<object> GetAllTransactionTagsAsync(Guid currentUserId);
        Task<object> UpdateTransactionTagAsync(Guid currentUserId, Guid transactionTagId, TransactionTagRequest request);
        Task<object> DeleteTransactionTagAsync(Guid currentUserId, Guid transactionTagId);
    }
}
