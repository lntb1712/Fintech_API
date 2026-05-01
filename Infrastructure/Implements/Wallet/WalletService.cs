using Azure.Storage.Blobs.Models;
using Common.Authorization.Utils;
using Common.Constant;
using Common.Settings;
using Common.UnitOfWork.UnitOfWorkPattern;
using Common.Utils;
using DomainService.Interfaces.Account;
using DomainService.Interfaces.Wallet;
using Entity.Entities.Wallet;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Model.RequestModel.Wallet;
using Model.ResponseModel.Wallet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Implements.Wallet
{
    public class WalletService(IUnitOfWork unitOfWork, IMemoryCache memoryCache, IUserService userService)
                : BaseService(unitOfWork, memoryCache), IWalletService
    {
        private readonly IUserService _userService = userService;

        public async Task<object> CreateWalletAsync(Guid currentUserId, WalletRequest request)
        {
            var user = await _userService.GetUserById(currentUserId);
            var newWallet = new FintechWallet
            {
                Id = Guid.NewGuid(),
                UserId = currentUserId,
                Name = request.Name,
                Type = request.Type,
                Currency = request.Currency,
                Balance = request.Balance,
                Status = request.Status,
                CreatedById = currentUserId,
                CreatedName = user.FullName,
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.Repository<FintechWallet>().AddAsync(newWallet);
            await _unitOfWork.SaveChangesAsync();
            return Utils.CreateResponseModel(new WalletResponse
            {
                Id = newWallet.Id,
                UserId = newWallet.UserId,
                Name = newWallet.Name,
                Type = newWallet.Type,
                Currency = newWallet.Currency,
                Balance = newWallet.Balance,
                Status = newWallet.Status
            });
        }

        public async Task<object> DeleteWalletAsync(Guid currentUserId, Guid walletId)
        {
            var user = await _userService.GetUserById(currentUserId);
            var existingWallet = await _unitOfWork.Repository<FintechWallet>().FirstOrDefaultAsync(x => x.Id == walletId)
                               ?? throw new KeyNotFoundException (string.Format(CommonMessage.Message_DataNotFound, "Wallet"));

            existingWallet.IsDeleted = true;
            existingWallet.UpdatedById = currentUserId;
            existingWallet.Updater = user.FullName;
            existingWallet.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Repository<FintechWallet>().Update(existingWallet);
            await _unitOfWork.SaveChangesAsync();
            return Utils.CreateResponseModel(new WalletResponse
            {
                Id = existingWallet.Id,
                UserId = existingWallet.UserId,
                Name = existingWallet.Name,
                Type = existingWallet.Type,
                Currency = existingWallet.Currency,
                Balance = existingWallet.Balance,
                Status = existingWallet.Status
            });
        }

        public async Task<object> GetAllWalletsAsync(Guid currentUserId)
        {
            var wallets = await _unitOfWork.Repository<FintechWallet>().Where(x => x.UserId == currentUserId && !x.IsDeleted)
                            .Select(x => new WalletResponse
                            {
                                Id = x.Id,
                                UserId = x.UserId,
                                Name = x.Name,
                                Type = x.Type,
                                Currency = x.Currency,
                                Balance = x.Balance,
                                Status = x.Status
                            }).ToListAsync();
            return Utils.CreateResponseModel(wallets, wallets.Count());
        }

        public async Task<object> GetWalletByIdAsync(Guid currentUserId, Guid walletId)
        {
            var wallet = await unitOfWork.Repository<FintechWallet>().FirstOrDefaultAsync(x => x.Id == walletId && x.UserId == currentUserId && !x.IsDeleted)
                            ?? throw new KeyNotFoundException(string.Format(CommonMessage.Message_DataNotFound, "Wallet"));

            return Utils.CreateResponseModel(new WalletResponse
            {
                Id = wallet.Id,
                UserId = wallet.UserId,
                Name = wallet.Name,
                Type = wallet.Type,
                Currency = wallet.Currency,
                Balance = wallet.Balance,
                Status = wallet.Status
            }, 1);
        }

        public async Task<object> UpdateWalletAsync(Guid currentUserId, Guid walletId, WalletRequest request)
        {
            var user = await _userService.GetUserById(currentUserId);

            var wallet = await _unitOfWork.Repository<FintechWallet>().FirstOrDefaultAsync(x => x.Id == walletId && x.UserId == currentUserId && !x.IsDeleted)
                               ?? throw new KeyNotFoundException(string.Format(CommonMessage.Message_DataNotFound, "Wallet"));

            wallet.Name = request.Name;
            wallet.Type = request.Type;
            wallet.Currency = request.Currency;
            wallet.Balance = request.Balance;
            wallet.Status = request.Status;
            wallet.UpdatedDate = DateTime.UtcNow;
            wallet.UpdatedById = currentUserId;
            wallet.Updater = user.FullName;

            _unitOfWork.Repository<FintechWallet>().Update(wallet);
            var result = await _unitOfWork.SaveChangesAsync();
            return Utils.CreateResponseModel(result, 1);
        }
    }
}
