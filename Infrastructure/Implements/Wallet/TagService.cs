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
    public class TagService(IUnitOfWork unitOfWork, IMemoryCache memoryCache, IUserService userService)
        : BaseService(unitOfWork, memoryCache), ITagService
    {
        private readonly IUserService _userService = userService;

        public async Task<object> CreateTagAsync(Guid currentUserId, TagRequest request)
        {
            var user = await _userService.GetUserById(currentUserId);
            var tag = new FintechTag
            {
                Id = Guid.NewGuid(),
                UserId = currentUserId,
                Name = request.Name,
                Color = request.Color,
                CreatedById = currentUserId,
                CreatedName = user.FullName,
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.Repository<FintechTag>().AddAsync(tag);
            await _unitOfWork.SaveChangesAsync();
            return Utils.CreateResponseModel(ToResponse(tag));
        }

        public async Task<object> DeleteTagAsync(Guid currentUserId, Guid tagId)
        {
            var user = await _userService.GetUserById(currentUserId);
            var tag = await GetOwnedTag(currentUserId, tagId);
            tag.IsDeleted = true;
            tag.UpdatedById = currentUserId;
            tag.Updater = user.FullName;
            tag.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Repository<FintechTag>().Update(tag);
            await _unitOfWork.SaveChangesAsync();
            return Utils.CreateResponseModel(ToResponse(tag));
        }

        public async Task<object> GetAllTagsAsync(Guid currentUserId)
        {
            var tags = await _unitOfWork.Repository<FintechTag>()
                .Where(x => x.UserId == currentUserId && !x.IsDeleted)
                .Select(x => new TagResponse
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    Name = x.Name,
                    Color = x.Color
                })
                .ToListAsync();

            return Utils.CreateResponseModel(tags, tags.Count);
        }

        public async Task<object> GetTagByIdAsync(Guid currentUserId, Guid tagId)
        {
            var tag = await GetOwnedTag(currentUserId, tagId);
            return Utils.CreateResponseModel(ToResponse(tag), 1);
        }

        public async Task<object> UpdateTagAsync(Guid currentUserId, Guid tagId, TagRequest request)
        {
            var user = await _userService.GetUserById(currentUserId);
            var tag = await GetOwnedTag(currentUserId, tagId);
            tag.Name = request.Name;
            tag.Color = request.Color;
            tag.UpdatedById = currentUserId;
            tag.Updater = user.FullName;
            tag.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.Repository<FintechTag>().Update(tag);
            await _unitOfWork.SaveChangesAsync();
            return Utils.CreateResponseModel(ToResponse(tag), 1);
        }

        private async Task<FintechTag> GetOwnedTag(Guid currentUserId, Guid tagId)
        {
            return await _unitOfWork.Repository<FintechTag>()
                .FirstOrDefaultAsync(x => x.Id == tagId && x.UserId == currentUserId && !x.IsDeleted)
                ?? throw new KeyNotFoundException(string.Format(CommonMessage.Message_DataNotFound, "Tag"));
        }

        private static TagResponse ToResponse(FintechTag tag)
        {
            return new TagResponse
            {
                Id = tag.Id,
                UserId = tag.UserId,
                Name = tag.Name,
                Color = tag.Color
            };
        }
    }
}
