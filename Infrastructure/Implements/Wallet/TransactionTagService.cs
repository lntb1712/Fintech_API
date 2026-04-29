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
    public class TransactionTagService(IUnitOfWork unitOfWork, IMemoryCache memoryCache, IUserService userService)
        : BaseService(unitOfWork, memoryCache), ITransactionTagService
    {
        private readonly IUserService _userService = userService;

        public async Task<object> CreateTransactionTagAsync(Guid currentUserId, TransactionTagRequest request)
        {
            var user = await _userService.GetUserById(currentUserId);
            await EnsureTag(currentUserId, request.TagId);

            var transactionTag = new FintechTransactionTag
            {
                Id = Guid.NewGuid(),
                TagId = request.TagId,
                Description = request.Description,
                CreatedById = currentUserId,
                CreatedName = user.FullName,
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.Repository<FintechTransactionTag>().AddAsync(transactionTag);
            await _unitOfWork.SaveChangesAsync();
            return Utils.CreateResponseModel(ToResponse(transactionTag));
        }

        public async Task<object> DeleteTransactionTagAsync(Guid currentUserId, Guid transactionTagId)
        {
            var user = await _userService.GetUserById(currentUserId);
            var transactionTag = await GetOwnedTransactionTag(currentUserId, transactionTagId);
            transactionTag.IsDeleted = true;
            transactionTag.UpdatedById = currentUserId;
            transactionTag.Updater = user.FullName;
            transactionTag.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Repository<FintechTransactionTag>().Update(transactionTag);
            await _unitOfWork.SaveChangesAsync();
            return Utils.CreateResponseModel(ToResponse(transactionTag));
        }

        public async Task<object> GetAllTransactionTagsAsync(Guid currentUserId)
        {
            var tagIds = _unitOfWork.Repository<FintechTag>()
                .Where(x => x.UserId == currentUserId && !x.IsDeleted)
                .Select(x => x.Id);

            var transactionTags = await _unitOfWork.Repository<FintechTransactionTag>()
                .Where(x => tagIds.Contains(x.TagId) && !x.IsDeleted)
                .Select(x => new TransactionTagResponse
                {
                    Id = x.Id,
                    TagId = x.TagId,
                    TagName = _unitOfWork.Repository<FintechTag>().Where(t => t.Id == x.TagId).Select(t => t.Name).FirstOrDefault(),
                    Description = x.Description
                })
                .ToListAsync();

            return Utils.CreateResponseModel(transactionTags, transactionTags.Count);
        }

        public async Task<object> GetTransactionTagByIdAsync(Guid currentUserId, Guid transactionTagId)
        {
            var transactionTag = await GetOwnedTransactionTag(currentUserId, transactionTagId);
            return Utils.CreateResponseModel(ToResponse(transactionTag), 1);
        }

        public async Task<object> UpdateTransactionTagAsync(Guid currentUserId, Guid transactionTagId, TransactionTagRequest request)
        {
            var user = await _userService.GetUserById(currentUserId);
            var transactionTag = await GetOwnedTransactionTag(currentUserId, transactionTagId);
            await EnsureTag(currentUserId, request.TagId);

            transactionTag.TagId = request.TagId;
            transactionTag.Description = request.Description;
            transactionTag.UpdatedById = currentUserId;
            transactionTag.Updater = user.FullName;
            transactionTag.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Repository<FintechTransactionTag>().Update(transactionTag);
            await _unitOfWork.SaveChangesAsync();
            return Utils.CreateResponseModel(ToResponse(transactionTag), 1);
        }

        private async Task<FintechTransactionTag> GetOwnedTransactionTag(Guid currentUserId, Guid transactionTagId)
        {
            var tagIds = _unitOfWork.Repository<FintechTag>()
                .Where(x => x.UserId == currentUserId && !x.IsDeleted)
                .Select(x => x.Id);

            return await _unitOfWork.Repository<FintechTransactionTag>()
                .FirstOrDefaultAsync(x => x.Id == transactionTagId && tagIds.Contains(x.TagId) && !x.IsDeleted)
                ?? throw new KeyNotFoundException(string.Format(CommonMessage.Message_DataNotFound, "Transaction tag"));
        }

        private async Task EnsureTag(Guid currentUserId, Guid tagId)
        {
            if (!await _unitOfWork.Repository<FintechTag>().AnyAsync(x => x.Id == tagId && x.UserId == currentUserId && !x.IsDeleted))
                throw new KeyNotFoundException(string.Format(CommonMessage.Message_DataNotFound, "Tag"));
        }

        private static TransactionTagResponse ToResponse(FintechTransactionTag transactionTag)
        {
            return new TransactionTagResponse
            {
                Id = transactionTag.Id,
                TagId = transactionTag.TagId,
                Description = transactionTag.Description
            };
        }
    }
}
