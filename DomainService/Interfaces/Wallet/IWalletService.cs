using Model.RequestModel.Wallet;
using Model.ResponseModel.Wallet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainService.Interfaces.Wallet
{
    public interface IWalletService
    {
        Task<object> CreateWalletAsync(Guid currentUserId, WalletRequest request);
        Task<object> GetWalletByIdAsync(Guid currentUserId, Guid walletId);
        Task<object> GetAllWalletsAsync(Guid currentUserId);
        Task<object> UpdateWalletAsync(Guid currentUserId, Guid walletId, WalletRequest request);
        Task<object> DeleteWalletAsync(Guid currentUserId, Guid walletId);
    }
}
