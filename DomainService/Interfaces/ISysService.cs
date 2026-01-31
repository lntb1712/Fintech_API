using Model.RequestModel;
using Model.RequestModel.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainService.Interfaces
{
    public interface ISysService
    {
        Task<object> SetAwaitingApproval(SysAppVersionRequest req);

        Task<object> GetAppVersion(string appName, string platform, string appBuild);

        Task<object> GetAwaitingApproval(string appName, string platform, string appBuild, int appVersion);

        Task<object> GetDeviceOfEmployee(UserDeviceRequest uDevice, Guid employeeId);
    }
}