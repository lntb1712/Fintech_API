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
    public class SharedWalletMemberService(IUnitOfWork unitOfWork, IMemoryCache memoryCache, IUserService userService)
        : BaseService(unitOfWork, memoryCache), ISharedWalletMemberService
    {
        private readonly IUserService _userService = userService;

        public async Task<object> CreateSharedWalletMemberAsync(Guid currentUserId, SharedWalletMemberRequest request)
        {
            var user = await _userService.GetUserById(currentUserId);
            await EnsureOwnedSharedWallet(currentUserId, request.SharedWalletId);

            var member = new FintechSharedWalletMember
            {
                Id = Guid.NewGuid(),
                SharedWalletId = request.SharedWalletId,
                UserId = request.UserId,
                CreatedById = currentUserId,
                CreatedName = user.FullName,
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.Repository<FintechSharedWalletMember>().AddAsync(member);
            await _unitOfWork.SaveChangesAsync();
            return Utils.CreateResponseModel(ToResponse(member));
        }

        public async Task<object> DeleteSharedWalletMemberAsync(Guid currentUserId, Guid sharedWalletMemberId)
        {
            var user = await _userService.GetUserById(currentUserId);
            var member = await GetAccessibleMember(currentUserId, sharedWalletMemberId);
            member.IsDeleted = true;
            member.UpdatedById = currentUserId;
            member.Updater = user.FullName;
            member.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Repository<FintechSharedWalletMember>().Update(member);
            await _unitOfWork.SaveChangesAsync();
            return Utils.CreateResponseModel(ToResponse(member));
        }

        public async Task<object> GetAllSharedWalletMembersAsync(Guid currentUserId)
        {
            var ownedSharedWalletIds = _unitOfWork.Repository<FintechSharedWallet>()
                .Where(x => x.OwnerId == currentUserId && !x.IsDeleted)
                .Select(x => x.Id);

            var members = await _unitOfWork.Repository<FintechSharedWalletMember>()
                .Where(x => !x.IsDeleted && (ownedSharedWalletIds.Contains(x.SharedWalletId) || x.UserId == currentUserId))
                .Select(x => new SharedWalletMemberResponse
                {
                    Id = x.Id,
                    SharedWalletId = x.SharedWalletId,
                    UserId = x.UserId
                })
                .ToListAsync();

            return Utils.CreateResponseModel(members, members.Count);
        }

        public async Task<object> GetSharedWalletMemberByIdAsync(Guid currentUserId, Guid sharedWalletMemberId)
        {
            var member = await GetAccessibleMember(currentUserId, sharedWalletMemberId);
            return Utils.CreateResponseModel(ToResponse(member), 1);
        }

        public async Task<object> UpdateSharedWalletMemberAsync(Guid currentUserId, Guid sharedWalletMemberId, SharedWalletMemberRequest request)
        {
            var user = await _userService.GetUserById(currentUserId);
            var member = await GetAccessibleMember(currentUserId, sharedWalletMemberId);
            await EnsureOwnedSharedWallet(currentUserId, request.SharedWalletId);

            member.SharedWalletId = request.SharedWalletId;
            member.UserId = request.UserId;
            member.UpdatedById = currentUserId;
            member.Updater = user.FullName;
            member.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Repository<FintechSharedWalletMember>().Update(member);
            await _unitOfWork.SaveChangesAsync();
            return Utils.CreateResponseModel(ToResponse(member), 1);
        }

        private async Task<FintechSharedWalletMember> GetAccessibleMember(Guid currentUserId, Guid sharedWalletMemberId)
        {
            var ownedSharedWalletIds = _unitOfWork.Repository<FintechSharedWallet>()
                .Where(x => x.OwnerId == currentUserId && !x.IsDeleted)
                .Select(x => x.Id);

            return await _unitOfWork.Repository<FintechSharedWalletMember>()
                .FirstOrDefaultAsync(x => x.Id == sharedWalletMemberId && !x.IsDeleted && (ownedSharedWalletIds.Contains(x.SharedWalletId) || x.UserId == currentUserId))
                ?? throw new KeyNotFoundException(string.Format(CommonMessage.Message_DataNotFound, "Shared wallet member"));
        }

        private async Task EnsureOwnedSharedWallet(Guid currentUserId, Guid sharedWalletId)
        {
            if (!await _unitOfWork.Repository<FintechSharedWallet>().AnyAsync(x => x.Id == sharedWalletId && x.OwnerId == currentUserId && !x.IsDeleted))
                throw new KeyNotFoundException(string.Format(CommonMessage.Message_DataNotFound, "Shared wallet"));
        }

        private static SharedWalletMemberResponse ToResponse(FintechSharedWalletMember member)
        {
            return new SharedWalletMemberResponse
            {
                Id = member.Id,
                SharedWalletId = member.SharedWalletId,
                UserId = member.UserId
            };
        }
    }
}
