using Model.RequestModel.PermissionManagement;
using Model.ResponseModel.PermissionManagement;

namespace DomainService.Interfaces.Account
{
    public interface IUserService
    {
        Task<object> GetList(Guid currentUserId, string keyword, int pageIndex, int pageSize);

        Task<object> GetDetail(Guid currentUserId, Guid id);

        Task<object> Create(Guid currentUserId, SysAccountRequest req);

        Task<object> Update(Guid currentUserId, Guid accountId, SysAccountRequest req);

        Task<object> Delete(Guid currentUserId, Guid id);

        Task<object> GetInfoMine(Guid currentUserId);

        Task<SysAccountResponse> GetUserById(Guid userId);
    }
}