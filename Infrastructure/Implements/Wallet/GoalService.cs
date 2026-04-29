using Common.Constant;
using Common.UnitOfWork.UnitOfWorkPattern;
using Common.Utils;
using DomainService.Interfaces.Account;
using DomainService.Interfaces.Wallet;
using Entity.Entities.Wallet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Model.RequestModel.Wallet;
using Model.ResponseModel.Wallet;

namespace Infrastructure.Implements.Wallet
{
    public class GoalService(IUnitOfWork unitOfWork, IMemoryCache memoryCache, IUserService userService)
        : BaseService(unitOfWork, memoryCache), IGoalService
    {
        private readonly IUserService _userService = userService;

        public async Task<object> CreateGoalAsync(Guid currentUserId, GoalRequest request)
        {
            var user = await _userService.GetUserById(currentUserId);
            var goal = new FintechGoal
            {
                Id = Guid.NewGuid(),
                UserId = currentUserId,
                Name = request.Name,
                TargetAmount = request.TargetAmount,
                CurrentAmount = request.CurrentAmount,
                StartDate = request.StartDate,
                TargetDate = request.TargetDate,
                Status = request.Status,
                Description = request.Description,
                CreatedById = currentUserId,
                CreatedName = user.FullName,
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.Repository<FintechGoal>().AddAsync(goal);
            await _unitOfWork.SaveChangesAsync();
            return Utils.CreateResponseModel(ToResponse(goal));
        }

        public async Task<object> DeleteGoalAsync(Guid currentUserId, Guid goalId)
        {
            var user = await _userService.GetUserById(currentUserId);
            var goal = await GetOwnedGoal(currentUserId, goalId);
            goal.IsDeleted = true;
            goal.UpdatedById = currentUserId;
            goal.Updater = user.FullName;
            goal.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Repository<FintechGoal>().Update(goal);
            await _unitOfWork.SaveChangesAsync();
            return Utils.CreateResponseModel(ToResponse(goal));
        }

        public async Task<object> GetAllGoalsAsync(Guid currentUserId)
        {
            var goals = await _unitOfWork.Repository<FintechGoal>()
                .Where(x => x.UserId == currentUserId && !x.IsDeleted)
                .Select(x => new GoalResponse
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    Name = x.Name,
                    TargetAmount = x.TargetAmount,
                    CurrentAmount = x.CurrentAmount,
                    StartDate = x.StartDate,
                    TargetDate = x.TargetDate,
                    Status = x.Status,
                    Description = x.Description
                })
                .ToListAsync();

            return Utils.CreateResponseModel(goals, goals.Count);
        }

        public async Task<object> GetGoalByIdAsync(Guid currentUserId, Guid goalId)
        {
            var goal = await GetOwnedGoal(currentUserId, goalId);
            return Utils.CreateResponseModel(ToResponse(goal), 1);
        }

        public async Task<object> UpdateGoalAsync(Guid currentUserId, Guid goalId, GoalRequest request)
        {
            var user = await _userService.GetUserById(currentUserId);
            var goal = await GetOwnedGoal(currentUserId, goalId);
            goal.Name = request.Name;
            goal.TargetAmount = request.TargetAmount;
            goal.CurrentAmount = request.CurrentAmount;
            goal.StartDate = request.StartDate;
            goal.TargetDate = request.TargetDate;
            goal.Status = request.Status;
            goal.Description = request.Description;
            goal.UpdatedById = currentUserId;
            goal.Updater = user.FullName;
            goal.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Repository<FintechGoal>().Update(goal);
            await _unitOfWork.SaveChangesAsync();
            return Utils.CreateResponseModel(ToResponse(goal), 1);
        }

        private async Task<FintechGoal> GetOwnedGoal(Guid currentUserId, Guid goalId)
        {
            return await _unitOfWork.Repository<FintechGoal>()
                .FirstOrDefaultAsync(x => x.Id == goalId && x.UserId == currentUserId && !x.IsDeleted)
                ?? throw new KeyNotFoundException(string.Format(CommonMessage.Message_DataNotFound, "Goal"));
        }

        private static GoalResponse ToResponse(FintechGoal goal)
        {
            return new GoalResponse
            {
                Id = goal.Id,
                UserId = goal.UserId,
                Name = goal.Name,
                TargetAmount = goal.TargetAmount,
                CurrentAmount = goal.CurrentAmount,
                StartDate = goal.StartDate,
                TargetDate = goal.TargetDate,
                Status = goal.Status,
                Description = goal.Description
            };
        }
    }
}
