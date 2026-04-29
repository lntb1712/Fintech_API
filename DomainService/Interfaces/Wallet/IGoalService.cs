using Model.RequestModel.Wallet;

namespace DomainService.Interfaces.Wallet
{
    public interface IGoalService
    {
        Task<object> CreateGoalAsync(Guid currentUserId, GoalRequest request);
        Task<object> GetGoalByIdAsync(Guid currentUserId, Guid goalId);
        Task<object> GetAllGoalsAsync(Guid currentUserId);
        Task<object> UpdateGoalAsync(Guid currentUserId, Guid goalId, GoalRequest request);
        Task<object> DeleteGoalAsync(Guid currentUserId, Guid goalId);
    }
}
