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
    public class BudgetService(IUnitOfWork unitOfWork, IMemoryCache memoryCache, IUserService userService)
        : BaseService(unitOfWork, memoryCache), IBudgetService
    {
        private readonly IUserService _userService = userService;

        public async Task<object> CreateBudgetAsync(Guid currentUserId, BudgetRequest request)
        {
            var user = await _userService.GetUserById(currentUserId);
            await EnsureWallet(currentUserId, request.WalletId);
            await EnsureCategory(currentUserId, request.CategoryId);

            var budget = new FintechBudget
            {
                Id = Guid.NewGuid(),
                UserId = currentUserId,
                WalletId = request.WalletId,
                CategoryId = request.CategoryId,
                Amount = request.Amount,
                Period = request.Period,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                CreatedById = currentUserId,
                CreatedName = user.FullName,
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.Repository<FintechBudget>().AddAsync(budget);
            await _unitOfWork.SaveChangesAsync();
            return Utils.CreateResponseModel(ToResponse(budget));
        }

        public async Task<object> DeleteBudgetAsync(Guid currentUserId, Guid budgetId)
        {
            var user = await _userService.GetUserById(currentUserId);
            var budget = await GetOwnedBudget(currentUserId, budgetId);
            budget.IsDeleted = true;
            budget.UpdatedById = currentUserId;
            budget.Updater = user.FullName;
            budget.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Repository<FintechBudget>().Update(budget);
            await _unitOfWork.SaveChangesAsync();
            return Utils.CreateResponseModel(ToResponse(budget));
        }

        public async Task<object> GetAllBudgetsAsync(Guid currentUserId)
        {
            var budgets = await _unitOfWork.Repository<FintechBudget>()
                .Where(x => x.UserId == currentUserId && !x.IsDeleted)
                .Select(x => new BudgetResponse
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    WalletId = x.WalletId,
                    WalletName = _unitOfWork.Repository<FintechWallet>().Where(w => w.Id == x.WalletId).Select(w => w.Name).FirstOrDefault(),
                    CategoryId = x.CategoryId,
                    CategoryName = _unitOfWork.Repository<FintechCategory>().Where(c => c.Id == x.CategoryId).Select(c => c.Name).FirstOrDefault(),
                    Amount = x.Amount,
                    Period = x.Period,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate
                })
                .ToListAsync();

            return Utils.CreateResponseModel(budgets, budgets.Count);
        }

        public async Task<object> GetBudgetByIdAsync(Guid currentUserId, Guid budgetId)
        {
            var budget = await GetOwnedBudget(currentUserId, budgetId);
            return Utils.CreateResponseModel(ToResponse(budget), 1);
        }

        public async Task<object> UpdateBudgetAsync(Guid currentUserId, Guid budgetId, BudgetRequest request)
        {
            var user = await _userService.GetUserById(currentUserId);
            var budget = await GetOwnedBudget(currentUserId, budgetId);
            await EnsureWallet(currentUserId, request.WalletId);
            await EnsureCategory(currentUserId, request.CategoryId);

            budget.WalletId = request.WalletId;
            budget.CategoryId = request.CategoryId;
            budget.Amount = request.Amount;
            budget.Period = request.Period;
            budget.StartDate = request.StartDate;
            budget.EndDate = request.EndDate;
            budget.UpdatedById = currentUserId;
            budget.Updater = user.FullName;
            budget.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Repository<FintechBudget>().Update(budget);
            await _unitOfWork.SaveChangesAsync();
            return Utils.CreateResponseModel(ToResponse(budget), 1);
        }

        private async Task<FintechBudget> GetOwnedBudget(Guid currentUserId, Guid budgetId)
        {
            return await _unitOfWork.Repository<FintechBudget>()
                .FirstOrDefaultAsync(x => x.Id == budgetId && x.UserId == currentUserId && !x.IsDeleted)
                ?? throw new KeyNotFoundException(string.Format(CommonMessage.Message_DataNotFound, "Budget"));
        }

        private async Task EnsureWallet(Guid currentUserId, Guid walletId)
        {
            if (!await _unitOfWork.Repository<FintechWallet>().AnyAsync(x => x.Id == walletId && x.UserId == currentUserId && !x.IsDeleted))
                throw new KeyNotFoundException(string.Format(CommonMessage.Message_DataNotFound, "Wallet"));
        }

        private async Task EnsureCategory(Guid currentUserId, Guid categoryId)
        {
            if (!await _unitOfWork.Repository<FintechCategory>().AnyAsync(x => x.Id == categoryId && x.UserId == currentUserId && !x.IsDeleted))
                throw new KeyNotFoundException(string.Format(CommonMessage.Message_DataNotFound, "Category"));
        }

        private static BudgetResponse ToResponse(FintechBudget budget)
        {
            return new BudgetResponse
            {
                Id = budget.Id,
                UserId = budget.UserId,
                WalletId = budget.WalletId,
                CategoryId = budget.CategoryId,
                Amount = budget.Amount,
                Period = budget.Period,
                StartDate = budget.StartDate,
                EndDate = budget.EndDate
            };
        }
    }
}
