using Model.RequestModel.Wallet;

namespace DomainService.Interfaces.Wallet
{
    public interface ITransactionService
    {
        Task<object> CreateTransactionAsync(Guid currentUserId, TransactionRequest request);
        Task<object> GetTransactionByIdAsync(Guid currentUserId, Guid transactionId);
        Task<object> GetAllTransactionsAsync(Guid currentUserId);
        Task<object> UpdateTransactionAsync(Guid currentUserId, Guid transactionId, TransactionRequest request);
        Task<object> DeleteTransactionAsync(Guid currentUserId, Guid transactionId);
    }
}
