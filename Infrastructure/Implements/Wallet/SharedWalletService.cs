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
    public class SharedWalletService(IUnitOfWork unitOfWork, IMemoryCache memoryCache, IUserService userService)
        : BaseService(unitOfWork, memoryCache), ISharedWalletService
    {
        private readonly IUserService _userService = userService;

        public async Task<object> CreateSharedWalletAsync(Guid currentUserId, SharedWalletRequest request)
        {
            var user = await _userService.GetUserById(currentUserId);
            await EnsureWallet(currentUserId, request.WalletId);
            var sharedWallet = new FintechSharedWallet
            {
                Id = Guid.NewGuid(),
                WalletId = request.WalletId,
                OwnerId = currentUserId,
                CreatedById = currentUserId,
                CreatedName = user.FullName,
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.Repository<FintechSharedWallet>().AddAsync(sharedWallet);
            await _unitOfWork.SaveChangesAsync();
            return Utils.CreateResponseModel(ToResponse(sharedWallet));
        }

        public async Task<object> DeleteSharedWalletAsync(Guid currentUserId, Guid sharedWalletId)
        {
            var user = await _userService.GetUserById(currentUserId);
            var sharedWallet = await GetOwnedSharedWallet(currentUserId, sharedWalletId);
            sharedWallet.IsDeleted = true;
            sharedWallet.UpdatedById = currentUserId;
            sharedWallet.Updater = user.FullName;
            sharedWallet.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Repository<FintechSharedWallet>().Update(sharedWallet);
            await _unitOfWork.SaveChangesAsync();
            return Utils.CreateResponseModel(ToResponse(sharedWallet));
        }

        public async Task<object> GetAllSharedWalletsAsync(Guid currentUserId)
        {
            var sharedWallets = await _unitOfWork.Repository<FintechSharedWallet>()
                .Where(x => x.OwnerId == currentUserId && !x.IsDeleted)
                .Select(x => new SharedWalletResponse
                {
                    Id = x.Id,
                    WalletId = x.WalletId,
                    WalletName = _unitOfWork.Repository<FintechWallet>().Where(w => w.Id == x.WalletId).Select(w => w.Name).FirstOrDefault(),
                    OwnerId = x.OwnerId
                })
                .ToListAsync();

            return Utils.CreateResponseModel(sharedWallets, sharedWallets.Count);
        }

        public async Task<object> GetSharedWalletByIdAsync(Guid currentUserId, Guid sharedWalletId)
        {
            var sharedWallet = await GetOwnedSharedWallet(currentUserId, sharedWalletId);
            return Utils.CreateResponseModel(ToResponse(sharedWallet), 1);
        }

        public async Task<object> UpdateSharedWalletAsync(Guid currentUserId, Guid sharedWalletId, SharedWalletRequest request)
        {
            var user = await _userService.GetUserById(currentUserId);
            var sharedWallet = await GetOwnedSharedWallet(currentUserId, sharedWalletId);
            await EnsureWallet(currentUserId, request.WalletId);

            sharedWallet.WalletId = request.WalletId;
            sharedWallet.OwnerId = currentUserId;
            sharedWallet.UpdatedById = currentUserId;
            sharedWallet.Updater = user.FullName;
            sharedWallet.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Repository<FintechSharedWallet>().Update(sharedWallet);
            await _unitOfWork.SaveChangesAsync();
            return Utils.CreateResponseModel(ToResponse(sharedWallet), 1);
        }

        private async Task<FintechSharedWallet> GetOwnedSharedWallet(Guid currentUserId, Guid sharedWalletId)
        {
            return await _unitOfWork.Repository<FintechSharedWallet>()
                .FirstOrDefaultAsync(x => x.Id == sharedWalletId && x.OwnerId == currentUserId && !x.IsDeleted)
                ?? throw new KeyNotFoundException(string.Format(CommonMessage.Message_DataNotFound, "Shared wallet"));
        }

        private async Task EnsureWallet(Guid currentUserId, Guid walletId)
        {
            if (!await _unitOfWork.Repository<FintechWallet>().AnyAsync(x => x.Id == walletId && x.UserId == currentUserId && !x.IsDeleted))
                throw new KeyNotFoundException(string.Format(CommonMessage.Message_DataNotFound, "Wallet"));
        }

        private static SharedWalletResponse ToResponse(FintechSharedWallet sharedWallet)
        {
            return new SharedWalletResponse
            {
                Id = sharedWallet.Id,
                WalletId = sharedWallet.WalletId,
                OwnerId = sharedWallet.OwnerId
            };
        }
    }
}
