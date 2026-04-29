using Common.Constant;
using Common.UnitOfWork.UnitOfWorkPattern;
using Common.Utils;
using DomainService.Interfaces.Wallet;
using Entity.Entities.Wallet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Model.RequestModel.Wallet;
using Model.ResponseModel.Wallet;

namespace Infrastructure.Implements.Wallet
{
    public class TransferService(IUnitOfWork unitOfWork, IMemoryCache memoryCache)
        : BaseService(unitOfWork, memoryCache), ITransferService
    {
        public async Task<object> CreateTransferAsync(Guid currentUserId, TransferRequest request)
        {
            var fromWallet = await GetOwnedWallet(currentUserId, request.FromWalletId);
            var toWallet = await GetOwnedWallet(currentUserId, request.ToWalletId);
            var transfer = new FintechTransfer
            {
                Id = Guid.NewGuid(),
                FromWalletId = request.FromWalletId,
                ToWalletId = request.ToWalletId,
                Amount = request.Amount,
                Description = request.Description
            };

            ApplyTransfer(fromWallet, toWallet, transfer.Amount);
            await _unitOfWork.Repository<FintechTransfer>().AddAsync(transfer);
            _unitOfWork.Repository<FintechWallet>().Update(fromWallet);
            _unitOfWork.Repository<FintechWallet>().Update(toWallet);
            await _unitOfWork.SaveChangesAsync();
            return Utils.CreateResponseModel(ToResponse(transfer));
        }

        public async Task<object> DeleteTransferAsync(Guid currentUserId, Guid transferId)
        {
            var transfer = await GetOwnedTransfer(currentUserId, transferId);
            var fromWallet = await GetOwnedWallet(currentUserId, transfer.FromWalletId);
            var toWallet = await GetOwnedWallet(currentUserId, transfer.ToWalletId);

            ReverseTransfer(fromWallet, toWallet, transfer.Amount);
            _unitOfWork.Repository<FintechTransfer>().Remove(transfer);
            _unitOfWork.Repository<FintechWallet>().Update(fromWallet);
            _unitOfWork.Repository<FintechWallet>().Update(toWallet);
            await _unitOfWork.SaveChangesAsync();
            return Utils.CreateResponseModel(ToResponse(transfer));
        }

        public async Task<object> GetAllTransfersAsync(Guid currentUserId)
        {
            var walletIds = _unitOfWork.Repository<FintechWallet>()
                .Where(x => x.UserId == currentUserId && !x.IsDeleted)
                .Select(x => x.Id);

            var transfers = await _unitOfWork.Repository<FintechTransfer>()
                .Where(x => walletIds.Contains(x.FromWalletId) || walletIds.Contains(x.ToWalletId))
                .Select(x => new TransferResponse
                {
                    Id = x.Id,
                    FromWalletId = x.FromWalletId,
                    FromWalletName = _unitOfWork.Repository<FintechWallet>().Where(w => w.Id == x.FromWalletId).Select(w => w.Name).FirstOrDefault(),
                    ToWalletId = x.ToWalletId,
                    ToWalletName = _unitOfWork.Repository<FintechWallet>().Where(w => w.Id == x.ToWalletId).Select(w => w.Name).FirstOrDefault(),
                    Amount = x.Amount,
                    Description = x.Description
                })
                .ToListAsync();

            return Utils.CreateResponseModel(transfers, transfers.Count);
        }

        public async Task<object> GetTransferByIdAsync(Guid currentUserId, Guid transferId)
        {
            var transfer = await GetOwnedTransfer(currentUserId, transferId);
            return Utils.CreateResponseModel(ToResponse(transfer), 1);
        }

        public async Task<object> UpdateTransferAsync(Guid currentUserId, Guid transferId, TransferRequest request)
        {
            var transfer = await GetOwnedTransfer(currentUserId, transferId);
            var oldFromWallet = await GetOwnedWallet(currentUserId, transfer.FromWalletId);
            var oldToWallet = await GetOwnedWallet(currentUserId, transfer.ToWalletId);
            var newFromWallet = transfer.FromWalletId == request.FromWalletId
                ? oldFromWallet
                : await GetOwnedWallet(currentUserId, request.FromWalletId);
            var newToWallet = transfer.ToWalletId == request.ToWalletId
                ? oldToWallet
                : await GetOwnedWallet(currentUserId, request.ToWalletId);

            ReverseTransfer(oldFromWallet, oldToWallet, transfer.Amount);
            transfer.FromWalletId = request.FromWalletId;
            transfer.ToWalletId = request.ToWalletId;
            transfer.Amount = request.Amount;
            transfer.Description = request.Description;
            ApplyTransfer(newFromWallet, newToWallet, transfer.Amount);

            _unitOfWork.Repository<FintechTransfer>().Update(transfer);
            _unitOfWork.Repository<FintechWallet>().Update(oldFromWallet);
            _unitOfWork.Repository<FintechWallet>().Update(oldToWallet);
            _unitOfWork.Repository<FintechWallet>().Update(newFromWallet);
            _unitOfWork.Repository<FintechWallet>().Update(newToWallet);
            await _unitOfWork.SaveChangesAsync();
            return Utils.CreateResponseModel(ToResponse(transfer), 1);
        }

        private async Task<FintechTransfer> GetOwnedTransfer(Guid currentUserId, Guid transferId)
        {
            var walletIds = _unitOfWork.Repository<FintechWallet>()
                .Where(x => x.UserId == currentUserId && !x.IsDeleted)
                .Select(x => x.Id);

            return await _unitOfWork.Repository<FintechTransfer>()
                .FirstOrDefaultAsync(x => x.Id == transferId && (walletIds.Contains(x.FromWalletId) || walletIds.Contains(x.ToWalletId)))
                ?? throw new KeyNotFoundException(string.Format(CommonMessage.Message_DataNotFound, "Transfer"));
        }

        private async Task<FintechWallet> GetOwnedWallet(Guid currentUserId, Guid walletId)
        {
            return await _unitOfWork.Repository<FintechWallet>()
                .FirstOrDefaultAsync(x => x.Id == walletId && x.UserId == currentUserId && !x.IsDeleted)
                ?? throw new KeyNotFoundException(string.Format(CommonMessage.Message_DataNotFound, "Wallet"));
        }

        private static void ApplyTransfer(FintechWallet fromWallet, FintechWallet toWallet, decimal amount)
        {
            fromWallet.Balance -= amount;
            toWallet.Balance += amount;
        }

        private static void ReverseTransfer(FintechWallet fromWallet, FintechWallet toWallet, decimal amount)
        {
            fromWallet.Balance += amount;
            toWallet.Balance -= amount;
        }

        private static TransferResponse ToResponse(FintechTransfer transfer)
        {
            return new TransferResponse
            {
                Id = transfer.Id,
                FromWalletId = transfer.FromWalletId,
                ToWalletId = transfer.ToWalletId,
                Amount = transfer.Amount,
                Description = transfer.Description
            };
        }
    }
}
