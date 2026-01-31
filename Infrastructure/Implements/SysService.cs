using Common.Constant;
using Common.UnitOfWork.UnitOfWorkPattern;
using Common.Utils;
using DomainService.Interfaces;
using DomainService.Interfaces.Account;
using Entity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Model.RequestModel;
using Model.RequestModel.Common;
using Model.ResponseModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Implements
{
    internal class SysService(IUnitOfWork unitOfWork, IMemoryCache memoryCache, IUserService userService)
           : BaseService(unitOfWork, memoryCache), ISysService
    {
        public async Task<object> GetAppVersion(string appName, string platform, string appBuild)
        {
            var exitsAppVerion = await _unitOfWork.Repository<SysAppVersion>()
                        .Where(s => s.AppName == appName && s.AppPlatform == platform && s.VersionName == appBuild)
                        .OrderByDescending(s => s.AppVersion)
                        .FirstOrDefaultAsync();

            if (exitsAppVerion == null)
                return Utils.CreateResponseModel(false, 0);

            return Utils.CreateResponseModel(exitsAppVerion.AppVersion, 0);
        }

        public async Task<object> GetAwaitingApproval(string appName, string platform, string appBuild, int appVersion)
        {
            var exitsAppVerion = await _unitOfWork.Repository<SysAppVersion>()
                                    .FirstOrDefaultAsync(s => s.AppName == appName && s.AppPlatform == platform && s.VersionName == appBuild)
                                    ?? throw new Exception(string.Format(CommonMessage.Message_DataNotFound, "App"));

            return Utils.CreateResponseModel(exitsAppVerion.IsAwaitingApproval, 0);
        }

        public async Task<object> GetDeviceOfEmployee(UserDeviceRequest uDevice, Guid employeeId)
        {
            var devices = await _unitOfWork.Repository<SysUserDevice>()
                .Where(d => d.UserId == employeeId && d.IsActivate == true && d.IsDeleted != true)
                .Select(s => new UserDeviceResponse()
                {
                    Id = s.Id,
                    EmployeeId = s.UserId,
                    DeviceUUID = s.DeviceUUID,
                    DevicePlatform = s.DevicePlatform,
                    DeviceOs = s.DeviceOS,
                    DeviceModel = s.DeviceModel,
                    DeviceName = s.DeviceName,
                    LastLogin = s.LastLogin,
                })
                .OrderByDescending(s => s.LastLogin)
                .ToListAsync();

            var distinctDevices = devices.DistinctBy(d => d.DeviceUUID)
                .Select(s => new
                {
                    Id = s.Id,
                    EmployeeId = s.EmployeeId,
                    DeviceUUID = s.DeviceUUID,
                    DevicePlatform = s.DevicePlatform,
                    DeviceModel = s.DeviceModel,
                    DeviceName = s.DeviceName,
                    IsCurrentDevice = s.DeviceUUID == uDevice.DeviceUUID,
                    LastLogin = s.LastLogin.HasValue ? s.LastLogin.Value.AddHours(7).ToString("dd/MM/yyyy HH:mm") : null,
                })
                .ToList();

            return Utils.CreateResponseModel(distinctDevices, devices.Count);
        }

        public async Task<object> SetAwaitingApproval(SysAppVersionRequest req)
        {
            bool isAddNew = false;
            var appVersion = await _unitOfWork.Repository<SysAppVersion>()
                .FirstOrDefaultAsync(p => p.AppName == req.AppName && p.AppPlatform == req.Platform && p.VersionName == req.VersionName && p.AppVersion == req.Version);

            if (appVersion == null)
            {
                appVersion = new SysAppVersion();
                isAddNew = true;
            }

            appVersion.AppName = req.AppName;
            appVersion.AppPlatform = req.Platform;
            appVersion.VersionName = req.VersionName;
            appVersion.MinVersion = req.Version;
            appVersion.AppVersion = req.Version;
            appVersion.DownloadPath = string.Empty;
            appVersion.ReleaseNotes = req.ReleaseNotes;
            appVersion.IsAwaitingApproval = req.IsAwaitingApproval;
            appVersion.IsForceDev = req.IsForceDev;
            appVersion.ModifiedDate = DateTime.Now;

            if (isAddNew)
            {
                appVersion.Id = Guid.NewGuid();
                _unitOfWork.Repository<SysAppVersion>().Add(appVersion);
            }
            else
                _unitOfWork.Repository<SysAppVersion>().Update(appVersion);

            await _unitOfWork.SaveChangesAsync();

            return Utils.CreateResponseModel(appVersion);
        }
    }
}