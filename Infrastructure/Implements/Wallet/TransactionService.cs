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
    public class TransactionService(IUnitOfWork unitOfWork, IMemoryCache memoryCache, IUserService userService)
        : BaseService(unitOfWork, memoryCache), ITransactionService
    {
        private readonly IUserService _userService = userService;

        public async Task<object> CreateTransactionAsync(Guid currentUserId, TransactionRequest request)
        {
            var user = await _userService.GetUserById(currentUserId);
            var wallet = await GetOwnedWallet(currentUserId, request.WalletId);
            await EnsureCategory(currentUserId, request.CategoryId);

            var transaction = new FintechTransaction
            {
                Id = Guid.NewGuid(),
                UserId = currentUserId,
                WalletId = request.WalletId,
                CategoryId = request.CategoryId,
                Amount = request.Amount,
                TransactionType = request.TransactionType,
                Description = request.Description,
                Source = request.Source,
                ExternalReference = request.ExternalReference,
                CreatedById = currentUserId,
                CreatedName = user.FullName,
                CreatedDate = DateTime.UtcNow
            };

            ApplyTransaction(wallet, transaction.TransactionType, transaction.Amount);
            await _unitOfWork.Repository<FintechTransaction>().AddAsync(transaction);
            _unitOfWork.Repository<FintechWallet>().Update(wallet);
            await _unitOfWork.SaveChangesAsync();
            return Utils.CreateResponseModel(ToResponse(transaction));
        }

        public async Task<object> DeleteTransactionAsync(Guid currentUserId, Guid transactionId)
        {
            var user = await _userService.GetUserById(currentUserId);
            var transaction = await GetOwnedTransaction(currentUserId, transactionId);
            var wallet = await GetOwnedWallet(currentUserId, transaction.WalletId);
            ReverseTransaction(wallet, transaction.TransactionType, transaction.Amount);

            transaction.IsDeleted = true;
            transaction.UpdatedById = currentUserId;
            transaction.Updater = user.FullName;
            transaction.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Repository<FintechTransaction>().Update(transaction);
            _unitOfWork.Repository<FintechWallet>().Update(wallet);
            await _unitOfWork.SaveChangesAsync();
            return Utils.CreateResponseModel(ToResponse(transaction));
        }

        public async Task<object> GetAllTransactionsAsync(Guid currentUserId)
        {
            var transactions = await _unitOfWork.Repository<FintechTransaction>()
                .Where(x => x.UserId == currentUserId && !x.IsDeleted)
                .Select(x => new TransactionResponse
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    WalletId = x.WalletId,
                    WalletName = _unitOfWork.Repository<FintechWallet>().Where(w => w.Id == x.WalletId).Select(w => w.Name).FirstOrDefault(),
                    CategoryId = x.CategoryId,
                    CategoryName = _unitOfWork.Repository<FintechCategory>().Where(c => c.Id == x.CategoryId).Select(c => c.Name).FirstOrDefault(),
                    Amount = x.Amount,
                    TransactionType = x.TransactionType,
                    Description = x.Description,
                    Source = x.Source,
                    ExternalReference = x.ExternalReference
                })
                .ToListAsync();

            return Utils.CreateResponseModel(transactions, transactions.Count);
        }

        public async Task<object> GetTransactionByIdAsync(Guid currentUserId, Guid transactionId)
        {
            var transaction = await GetOwnedTransaction(currentUserId, transactionId);
            return Utils.CreateResponseModel(ToResponse(transaction), 1);
        }

        public async Task<object> UpdateTransactionAsync(Guid currentUserId, Guid transactionId, TransactionRequest request)
        {
            var user = await _userService.GetUserById(currentUserId);
            var transaction = await GetOwnedTransaction(currentUserId, transactionId);
            var oldWallet = await GetOwnedWallet(currentUserId, transaction.WalletId);
            var newWallet = transaction.WalletId == request.WalletId
                ? oldWallet
                : await GetOwnedWallet(currentUserId, request.WalletId);
            await EnsureCategory(currentUserId, request.CategoryId);

            ReverseTransaction(oldWallet, transaction.TransactionType, transaction.Amount);
            transaction.WalletId = request.WalletId;
            transaction.CategoryId = request.CategoryId;
            transaction.Amount = request.Amount;
            transaction.TransactionType = request.TransactionType;
            transaction.Description = request.Description;
            transaction.Source = request.Source;
            transaction.ExternalReference = request.ExternalReference;
            transaction.UpdatedById = currentUserId;
            transaction.Updater = user.FullName;
            transaction.UpdatedDate = DateTime.UtcNow;
            ApplyTransaction(newWallet, transaction.TransactionType, transaction.Amount);

            _unitOfWork.Repository<FintechTransaction>().Update(transaction);
            _unitOfWork.Repository<FintechWallet>().Update(oldWallet);
            _unitOfWork.Repository<FintechWallet>().Update(newWallet);
            await _unitOfWork.SaveChangesAsync();
            return Utils.CreateResponseModel(ToResponse(transaction), 1);
        }

        private async Task<FintechTransaction> GetOwnedTransaction(Guid currentUserId, Guid transactionId)
        {
            return await _unitOfWork.Repository<FintechTransaction>()
                .FirstOrDefaultAsync(x => x.Id == transactionId && x.UserId == currentUserId && !x.IsDeleted)
                ?? throw new KeyNotFoundException(string.Format(CommonMessage.Message_DataNotFound, "Transaction"));
        }

        private async Task<FintechWallet> GetOwnedWallet(Guid currentUserId, Guid walletId)
        {
            return await _unitOfWork.Repository<FintechWallet>()
                .FirstOrDefaultAsync(x => x.Id == walletId && x.UserId == currentUserId && !x.IsDeleted)
                ?? throw new KeyNotFoundException(string.Format(CommonMessage.Message_DataNotFound, "Wallet"));
        }

        private async Task EnsureCategory(Guid currentUserId, Guid categoryId)
        {
            if (!await _unitOfWork.Repository<FintechCategory>().AnyAsync(x => x.Id == categoryId && x.UserId == currentUserId && !x.IsDeleted))
                throw new KeyNotFoundException(string.Format(CommonMessage.Message_DataNotFound, "Category"));
        }

        private static void ApplyTransaction(FintechWallet wallet, string type, decimal amount)
        {
            if (type.Equals("INCOME", StringComparison.OrdinalIgnoreCase))
                wallet.Balance += amount;
            else if (type.Equals("EXPENSE", StringComparison.OrdinalIgnoreCase))
                wallet.Balance -= amount;
        }

        private static void ReverseTransaction(FintechWallet wallet, string type, decimal amount)
        {
            if (type.Equals("INCOME", StringComparison.OrdinalIgnoreCase))
                wallet.Balance -= amount;
            else if (type.Equals("EXPENSE", StringComparison.OrdinalIgnoreCase))
                wallet.Balance += amount;
        }

        private static TransactionResponse ToResponse(FintechTransaction transaction)
        {
            return new TransactionResponse
            {
                Id = transaction.Id,
                UserId = transaction.UserId,
                WalletId = transaction.WalletId,
                CategoryId = transaction.CategoryId,
                Amount = transaction.Amount,
                TransactionType = transaction.TransactionType,
                Description = transaction.Description,
                Source = transaction.Source,
                ExternalReference = transaction.ExternalReference
            };
        }
    }
}
