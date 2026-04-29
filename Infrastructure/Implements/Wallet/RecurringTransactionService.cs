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
    public class RecurringTransactionService(IUnitOfWork unitOfWork, IMemoryCache memoryCache, IUserService userService)
        : BaseService(unitOfWork, memoryCache), IRecurringTransactionService
    {
        private readonly IUserService _userService = userService;

        public async Task<object> CreateRecurringTransactionAsync(Guid currentUserId, RecurringTransactionRequest request)
        {
            var user = await _userService.GetUserById(currentUserId);
            await EnsureWallet(currentUserId, request.WalletId);
            await EnsureCategory(currentUserId, request.CategoryId);

            var recurringTransaction = new FintechRecurringTransaction
            {
                Id = Guid.NewGuid(),
                UserId = currentUserId,
                WalletId = request.WalletId,
                CategoryId = request.CategoryId,
                Amount = request.Amount,
                TransactionType = request.TransactionType,
                Frequency = request.Frequency,
                IntervalValue = request.IntervalValue,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                NextRunDate = request.NextRunDate,
                Status = request.Status,
                Description = request.Description,
                CreatedById = currentUserId,
                CreatedName = user.FullName,
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.Repository<FintechRecurringTransaction>().AddAsync(recurringTransaction);
            await _unitOfWork.SaveChangesAsync();
            return Utils.CreateResponseModel(ToResponse(recurringTransaction));
        }

        public async Task<object> DeleteRecurringTransactionAsync(Guid currentUserId, Guid recurringTransactionId)
        {
            var user = await _userService.GetUserById(currentUserId);
            var recurringTransaction = await GetOwnedRecurringTransaction(currentUserId, recurringTransactionId);
            recurringTransaction.IsDeleted = true;
            recurringTransaction.UpdatedById = currentUserId;
            recurringTransaction.Updater = user.FullName;
            recurringTransaction.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Repository<FintechRecurringTransaction>().Update(recurringTransaction);
            await _unitOfWork.SaveChangesAsync();
            return Utils.CreateResponseModel(ToResponse(recurringTransaction));
        }

        public async Task<object> GetAllRecurringTransactionsAsync(Guid currentUserId)
        {
            var recurringTransactions = await _unitOfWork.Repository<FintechRecurringTransaction>()
                .Where(x => x.UserId == currentUserId && !x.IsDeleted)
                .Select(x => new RecurringTransactionResponse
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    WalletId = x.WalletId,
                    WalletName = _unitOfWork.Repository<FintechWallet>().Where(w => w.Id == x.WalletId).Select(w => w.Name).FirstOrDefault(),
                    CategoryId = x.CategoryId,
                    CategoryName = _unitOfWork.Repository<FintechCategory>().Where(c => c.Id == x.CategoryId).Select(c => c.Name).FirstOrDefault(),
                    Amount = x.Amount,
                    TransactionType = x.TransactionType,
                    Frequency = x.Frequency,
                    IntervalValue = x.IntervalValue,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    NextRunDate = x.NextRunDate,
                    Status = x.Status,
                    Description = x.Description
                })
                .ToListAsync();

            return Utils.CreateResponseModel(recurringTransactions, recurringTransactions.Count);
        }

        public async Task<object> GetRecurringTransactionByIdAsync(Guid currentUserId, Guid recurringTransactionId)
        {
            var recurringTransaction = await GetOwnedRecurringTransaction(currentUserId, recurringTransactionId);
            return Utils.CreateResponseModel(ToResponse(recurringTransaction), 1);
        }

        public async Task<object> UpdateRecurringTransactionAsync(Guid currentUserId, Guid recurringTransactionId, RecurringTransactionRequest request)
        {
            var user = await _userService.GetUserById(currentUserId);
            var recurringTransaction = await GetOwnedRecurringTransaction(currentUserId, recurringTransactionId);
            await EnsureWallet(currentUserId, request.WalletId);
            await EnsureCategory(currentUserId, request.CategoryId);

            recurringTransaction.WalletId = request.WalletId;
            recurringTransaction.CategoryId = request.CategoryId;
            recurringTransaction.Amount = request.Amount;
            recurringTransaction.TransactionType = request.TransactionType;
            recurringTransaction.Frequency = request.Frequency;
            recurringTransaction.IntervalValue = request.IntervalValue;
            recurringTransaction.StartDate = request.StartDate;
            recurringTransaction.EndDate = request.EndDate;
            recurringTransaction.NextRunDate = request.NextRunDate;
            recurringTransaction.Status = request.Status;
            recurringTransaction.Description = request.Description;
            recurringTransaction.UpdatedById = currentUserId;
            recurringTransaction.Updater = user.FullName;
            recurringTransaction.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Repository<FintechRecurringTransaction>().Update(recurringTransaction);
            await _unitOfWork.SaveChangesAsync();
            return Utils.CreateResponseModel(ToResponse(recurringTransaction), 1);
        }

        private async Task<FintechRecurringTransaction> GetOwnedRecurringTransaction(Guid currentUserId, Guid recurringTransactionId)
        {
            return await _unitOfWork.Repository<FintechRecurringTransaction>()
                .FirstOrDefaultAsync(x => x.Id == recurringTransactionId && x.UserId == currentUserId && !x.IsDeleted)
                ?? throw new KeyNotFoundException(string.Format(CommonMessage.Message_DataNotFound, "Recurring transaction"));
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

        private static RecurringTransactionResponse ToResponse(FintechRecurringTransaction recurringTransaction)
        {
            return new RecurringTransactionResponse
            {
                Id = recurringTransaction.Id,
                UserId = recurringTransaction.UserId,
                WalletId = recurringTransaction.WalletId,
                CategoryId = recurringTransaction.CategoryId,
                Amount = recurringTransaction.Amount,
                TransactionType = recurringTransaction.TransactionType,
                Frequency = recurringTransaction.Frequency,
                IntervalValue = recurringTransaction.IntervalValue,
                StartDate = recurringTransaction.StartDate,
                EndDate = recurringTransaction.EndDate,
                NextRunDate = recurringTransaction.NextRunDate,
                Status = recurringTransaction.Status,
                Description = recurringTransaction.Description
            };
        }
    }
}
