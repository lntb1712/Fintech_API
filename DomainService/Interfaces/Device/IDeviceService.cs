using Model.RequestModel;
using Model.RequestModel.Common;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainService.Interfaces.Device
{
    public interface IDeviceService
    {
        Task<object> GetListDevice(Guid currentUserId, string currentUserName, string keyword, int pageIndex, int pageSize);
        Task<object> GetDetail(Guid currentUserId, string currentUserName, Guid id);
        Task<object> Update(Guid currentUserId, string currentUserName, Guid id, SysDeviceRequest req);
        Task<object> Delete(Guid currentUserId, string currentUserName, Guid id);
        Task<object> Create(Guid currentUserId, string currentUserName, DeviceInfoRequest deviceRequest,SysDeviceRequest req);

    }
}
