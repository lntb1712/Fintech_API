using Model.RequestModel.Wallet;

namespace DomainService.Interfaces.Wallet
{
    public interface ITagService
    {
        Task<object> CreateTagAsync(Guid currentUserId, TagRequest request);
        Task<object> GetTagByIdAsync(Guid currentUserId, Guid tagId);
        Task<object> GetAllTagsAsync(Guid currentUserId);
        Task<object> UpdateTagAsync(Guid currentUserId, Guid tagId, TagRequest request);
        Task<object> DeleteTagAsync(Guid currentUserId, Guid tagId);
    }
}
