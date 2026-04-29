using Model.RequestModel.Wallet;

namespace DomainService.Interfaces.Wallet
{
    public interface IBudgetService
    {
        Task<object> CreateBudgetAsync(Guid currentUserId, BudgetRequest request);
        Task<object> GetBudgetByIdAsync(Guid currentUserId, Guid budgetId);
        Task<object> GetAllBudgetsAsync(Guid currentUserId);
        Task<object> UpdateBudgetAsync(Guid currentUserId, Guid budgetId, BudgetRequest request);
        Task<object> DeleteBudgetAsync(Guid currentUserId, Guid budgetId);
    }
}
