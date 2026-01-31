using Common.Authorization.Utils;
using Common.Constant;
using Common.UnitOfWork.UnitOfWorkPattern;
using Common.Utils;
using DomainService.Interfaces.Account;
using DomainService.Interfaces.Device;
using Entity.Entities;
using Entity.Entities.Account;
using Microsoft.EntityFrameworkCore;
using Microsoft.Exchange.WebServices.Data;
using Microsoft.Extensions.Caching.Memory;
using Model.RequestModel;
using Model.RequestModel.Common;
using Model.ResponseModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Implements.Device
{
    public class DeviceService(IUnitOfWork unitOfWork, IMemoryCache memoryCache,IAuthService authService)
        : BaseService(unitOfWork, memoryCache), IDeviceService
    {
        private readonly IAuthService _authService = authService;
        public async Task<object> Create(Guid currentUserId, string currentUserName, DeviceInfoRequest deviceRequest, SysDeviceRequest req)
        {
            var canCreate = await _authService.VerifyPermissionOfUser(currentUserId, "DEVICE", "C");

            if (!canCreate)
                throw new UnauthorizedAccessException("Bạn không có quyền tạo thiết bị mới");

            var existDevice = await _unitOfWork.Repository<SysDevice>().FirstOrDefaultAsync(r => r.IsDeleted != true && req.UDID.Equals(r.UDID));

            if (existDevice != null)
                throw new KeyExistsException(string.Format(CommonMessage.Message_Exists, "Device"));

            var device = new SysDevice
            {
                Id = Guid.NewGuid(),
                UserId = req.UserId,
                UDID = req.UDID?? "",
                OSName = deviceRequest.OSName ?? "",
                OSVersion = deviceRequest.OSVersion??"",
                DeviceType = deviceRequest.DeviceType??"",
                DeviceName = deviceRequest.DeviceName?? "",
                DeviceDescription= "",
                IsActive = true,
                CreatedById = currentUserId,
                CreatedDate = DateTime.Now,
                CreatedName =  currentUserName,
                RefreshToken ="",
                RfTokenRevokedTime = new DateTime(1753, 1, 1),
                RfTokenCreateTime = new DateTime(1753, 1, 1),
                RfTokenExpiryTime = new DateTime(1753, 1, 1),
                RfTokenCreatedByIp = "",
            };
            _unitOfWork.Repository<SysDevice>().Add(device);
            var res = await _unitOfWork.SaveChangesAsync();

            return Utils.CreateResponseModel(res > 0);

        }

        public async Task<object> Delete(Guid currentUserId, string currentUserName, Guid id)
        {
            var canDelete = await _authService.VerifyPermissionOfUser(currentUserId, "DEVICE", "D");

            if (!canDelete)
                throw new UnauthorizedAccessException("Bạn không có quyền xóa thiết bị");
            var existDevice = await _unitOfWork.Repository<SysDevice>()
                .FirstOrDefaultAsync(x => x.IsDeleted != true && x.Id == id)
                ?? throw new KeyNotFoundException( string.Format(CommonMessage.Message_NotFound,"Device"));

            existDevice.IsDeleted = true;
            existDevice.UpdatedById = currentUserId;
            existDevice.UpdatedDate = DateTime.Now;
            existDevice.Updater = currentUserName;

            var userDevice = await _unitOfWork.Repository<SysUserDevice>().Where(x => x.IsDeleted != true && x.Id == id).ToListAsync();

            userDevice.ForEach(ud =>
            {
                ud.IsDeleted = true;
                ud.UpdatedById = currentUserId;
                ud.UpdatedDate = DateTime.Now;
                ud.Updater = currentUserName;
            });
            _unitOfWork.Repository<SysUserDevice>().UpdateRange(userDevice);
            _unitOfWork.Repository<SysDevice>().Update(existDevice);
            var res = await _unitOfWork.SaveChangesAsync();
            return Utils.CreateResponseModel(res > 0);
            }

        public async Task<object> GetDetail(Guid currentUserId, string currentUserName, Guid id)
        {

            var detail = await _unitOfWork.Repository<SysDevice>()
                .Where( x=>x.IsDeleted!=true && x.Id== id)
                .Select(r=> new SysDeviceResponse
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    UDID = r.UDID,
                    OSVersion = r.OSVersion,
                    OSName = r.OSName,
                    DeviceName = r.DeviceName,
                    DeviceType = r.DeviceType,
                    DeviceDescription = r.DeviceDescription,
                    IsActive = r.IsActive,
                    RfTokenCreateTime = r.RfTokenCreateTime,
                    RfTokenExpiryTime = r.RfTokenExpiryTime,
                    RfTokenRevokedTime = r.RfTokenRevokedTime,
                    RfTokenCreatedByIp = r.RfTokenCreatedByIp,
                    RfTokenRevokedByIp = r.RfTokenRevokedByIp,
                    CreatedDate = r.CreatedDate,
                    Creator = r.CreatedName,
                }).FirstOrDefaultAsync()
                ?? throw new KeyNotFoundException( string.Format(CommonMessage.Message_NotFound,"Device"));
            return Utils.CreateResponseModel(detail,1);
        }

        public async Task<object> GetListDevice(Guid currentUserId, string currentUserName, string keyword, int pageIndex, int pageSize)
        {
            var canRead = await _authService.VerifyPermissionOfUser(currentUserId, "DEVICE", "R");

            if (!canRead)
                throw new UnauthorizedAccessException("Bạn không có quyền lấy danh sách thiết bị");
            keyword = keyword.ToLower();
            var query = _unitOfWork.Repository<SysDevice>()
                .Where(d => d.IsDeleted != true &&
                            (d.DeviceName!.ToLower().Contains(keyword)))
                .OrderByDescending(d => d.CreatedDate)

                .Select(r => new SysDeviceResponse
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    UDID = r.UDID,
                    OSVersion = r.OSVersion,
                    OSName = r.OSName,
                    DeviceName = r.DeviceName,
                    DeviceType = r.DeviceType,
                    DeviceDescription = r.DeviceDescription,
                    IsActive = r.IsActive,
                    RfTokenCreateTime = r.RfTokenCreateTime,
                    RfTokenExpiryTime = r.RfTokenExpiryTime,
                    RfTokenRevokedTime = r.RfTokenRevokedTime,
                    RfTokenCreatedByIp = r.RfTokenCreatedByIp,
                    RfTokenRevokedByIp = r.RfTokenRevokedByIp,
                    CreatedDate = r.CreatedDate,
                    Creator = r.CreatedName,

                });
            var data = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).OrderByDescending(d => d.CreatedDate).ToListAsync();
            return Utils.CreateResponseModel(data, await query.CountAsync());
        }

        public async Task<object> Update(Guid currentUserId, string currentUserName, Guid id, SysDeviceRequest req)
        {
            var canUpdate = await _authService.VerifyPermissionOfUser(currentUserId, "DEVICE", "U");

            if (!canUpdate)
                throw new UnauthorizedAccessException("Bạn không có quyền cập nhật thiết bị");
            var existDevice = await _unitOfWork.Repository<SysDevice>()
                .FirstOrDefaultAsync(x => x.IsDeleted != true && x.Id != id);
            if (existDevice !=null)
            {
                throw new KeyExistsException(string.Format(CommonMessage.Message_Exists, "Device"));
            }
            var device = await _unitOfWork.Repository<SysDevice>().FirstOrDefaultAsync(x => x.IsDeleted != true && x.UDID == req.UDID && x.Id== id);
            device.IsActive = req.IsActive;
            device.UpdatedById = currentUserId;
            device.UpdatedDate = DateTime.Now;

            _unitOfWork.Repository<SysDevice>().Update(device);
            var res = await _unitOfWork.SaveChangesAsync();
            return Utils.CreateResponseModel(res > 0);
        }
    }
}
