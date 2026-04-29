using Model.RequestModel.Wallet;

namespace DomainService.Interfaces.Wallet
{
    public interface IRecurringTransactionService
    {
        Task<object> CreateRecurringTransactionAsync(Guid currentUserId, RecurringTransactionRequest request);
        Task<object> GetRecurringTransactionByIdAsync(Guid currentUserId, Guid recurringTransactionId);
        Task<object> GetAllRecurringTransactionsAsync(Guid currentUserId);
        Task<object> UpdateRecurringTransactionAsync(Guid currentUserId, Guid recurringTransactionId, RecurringTransactionRequest request);
        Task<object> DeleteRecurringTransactionAsync(Guid currentUserId, Guid recurringTransactionId);
    }
}
