using Azure.Storage.Sas;
using Common.Authorization.Utils;
using Common.Constant;
using Common.UnitOfWork.UnitOfWorkPattern;
using Common.Utils;
using DomainService.Interfaces.Account;
using DomainService.Interfaces.File;
using DomainService.Interfaces.PermissionManagement;
using Entity.Entities.Account;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Model.RequestModel.PermissionManagement;
using Model.ResponseModel.PermissionManagement;
using System.Net.WebSockets;

namespace Infrastructure.Implements.Account
{
    public class UserService(IUnitOfWork unitOfWork, IMemoryCache memoryCache, IAuthService authService, IFileService fileService, ISysUserActivitiesService sysUserActivitiesService) : BaseService(unitOfWork, memoryCache), IUserService
    {
        private readonly IFileService _fileService = fileService;
        private readonly IAuthService _authService = authService;
        private readonly ISysUserActivitiesService _sysUserActivitiesService = sysUserActivitiesService;

        public async Task<object> Create(Guid currentUserId, SysAccountRequest req)
        {
            var canCreate = await _authService.VerifyPermissionOfUser(currentUserId, "USER", "C");
            var currentUser = await GetUserById(currentUserId);

            if (!canCreate)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền tạo người dùng mới");
            }

            var existAccount = await _unitOfWork.Repository<SysAccount>()
                                .FirstOrDefaultAsync(a => a.IsDeleted != true &&
                                (req.Code.Equals(a.Code) || (req.UserName.Equals(a.UserName))));

            if (existAccount != null)
            {
                throw new KeyExistsException(string.Format(CommonMessage.Message_Exists, "Account"));
            }

            List<IFormFile> lstAvatar = new List<IFormFile>();
            lstAvatar.Add(req.Avatar);

            var urlAvatar = await _fileService.AzureBlobUploadFiles(lstAvatar, currentUserId);

            var account = new SysAccount
            {
                Id = Guid.NewGuid(),
                Code = req.Code,
                UserName = req.UserName,
                Password = Utils.HashMd5($"{req.Code}_{req.UserName}"),
                Email = req.Email,
                FirstName = req.FirstName,
                LastName = req.LastName,
                PhoneNumber = req.PhoneNumber,
                Group = req.Group,
                Base = req.Base,
                Note = req.Note,
                Avatar = "",
                FullNameNoAccent = req.UserName,
                CreatedById = currentUserId,
                CreatedDate = DateTime.Now,
                CreatedName = currentUser.FullName,
                IsDeleted = false,
                Updater = currentUser.FullName,
                UpdatedDate = DateTime.Now,
            };
            try
            {
                _unitOfWork.Repository<SysAccount>().Add(account);
                var res = await _unitOfWork.SaveChangesAsync();

                return Utils.CreateResponseModel(res > 0);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Lỗi:{ex.Message}");
            }
        }

        public async Task<object> Delete(Guid currentUserId, Guid id)
        {
            var currentUser = await GetUserById(currentUserId);
            var canDelete = await _authService.VerifyPermissionOfUser(currentUserId, "USER", "D");
            if (!canDelete)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền xóa người dùng");
            }
            var existAccount = await _unitOfWork.Repository<SysAccount>().FirstOrDefaultAsync(x => x.IsDeleted != true && x.Id == id)
                                    ?? throw new KeyNotFoundException(string.Format(CommonMessage.Message_NotFound, "Account"));
            existAccount.IsDeleted = true;
            existAccount.Updater = currentUser.FullName;
            existAccount.UpdatedDate = DateTime.Now;
            existAccount.UpdatedById = currentUserId;

            _unitOfWork.Repository<SysAccount>().Update(existAccount);
            var res = await _unitOfWork.SaveChangesAsync();

            return Utils.CreateResponseModel(res > 0);
        }

        public async Task<object> GetDetail(Guid currentUserId, Guid id)
        {
            var detail = await _unitOfWork.Repository<SysAccount>()
                .Where(a => a.IsDeleted != true && a.Id == id)
                .Select(r => new SysAccountResponse
                {
                    Id = r.Id,
                    Code = r.Code,
                    UserName = r.UserName,
                    Email = r.Email,
                    PhoneNumber = r.PhoneNumber,
                    FirstName = r.FirstName,
                    LastName = r.LastName,
                    Group = r.Group,
                    Base = r.Base,
                    Note = r.Note,
                    FullNameNoAccent = r.FullNameNoAccent,
                    Avatar = r.Avatar ?? "",
                    CreatedDate = r.CreatedDate,
                    Creator = r.CreatedName,
                }).FirstOrDefaultAsync()
                ?? throw new KeyNotFoundException(string.Format(CommonMessage.Message_NotFound, "Account")); ;

            return Utils.CreateResponseModel(detail, 1);
        }

        public async Task<object> GetInfoMine(Guid currentUserId)
        {
            // GET USER PERMISSION
            var lstPermOfUser = await _sysUserActivitiesService.GetPermissionOfUser(currentUserId);
            var detail = await _unitOfWork.Repository<SysAccount>()
               .Where(a => a.IsDeleted != true && a.Id == currentUserId)
               .Select(r => new SysAccountResponse
               {
                   Id = r.Id,
                   Code = r.Code,
                   UserName = r.UserName,
                   Email = r.Email,
                   PhoneNumber = r.PhoneNumber,
                   FirstName = r.FirstName,
                   LastName = r.LastName,
                   Group = r.Group,
                   Base = r.Base,
                   Note = r.Note,
                   FullNameNoAccent = r.FullNameNoAccent,
                   Avatar = r.Avatar ?? "",
                   CreatedDate = r.CreatedDate,
                   Creator = r.CreatedName,
                   LstUserPermission = lstPermOfUser
               }).FirstOrDefaultAsync()
               ?? throw new KeyNotFoundException(string.Format(CommonMessage.Message_NotFound, "Account")); ;
            return Utils.CreateResponseModel(detail, 1);
        }

        public async Task<object> GetList(Guid currentUserId, string keyword, int pageIndex, int pageSize)
        {
            var canRead = await _authService.VerifyPermissionOfUser(currentUserId, "USER", "R");
            if (!canRead)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền xem danh sách người dùng");
            }
            keyword = keyword.ToLower();
            var query = _unitOfWork.Repository<SysAccount>()
                                   .Where(r => r.IsDeleted != true &&
                                   (r.FullNameNoAccent!.ToLower().Contains(keyword)));

            var data = await query.OrderByDescending(d => d.CreatedDate)
                                  .Skip((pageIndex - 1) * pageSize)
                                  .Take(pageSize)
                                  .Select(r => new SysAccountResponse
                                  {
                                      Id = r.Id,
                                      Code = r.Code,
                                      UserName = r.UserName,
                                      Email = r.Email,
                                      PhoneNumber = r.PhoneNumber,
                                      FirstName = r.FirstName,
                                      LastName = r.LastName,
                                      Group = r.Group,
                                      Base = r.Base,
                                      Note = r.Note,
                                      FullNameNoAccent = r.FullNameNoAccent,
                                      Avatar = r.Avatar ?? "",
                                      CreatedDate = r.CreatedDate,
                                      Creator = r.CreatedName,
                                  }).ToListAsync();

            var totalCount = await query.CountAsync();
            return Utils.CreateResponseModel(data, totalCount);
        }

        public async Task<object> Update(Guid currentUserId, Guid accountId, SysAccountRequest req)
        {
            var currentUserName = (await GetUserById(currentUserId)).FullName;
            var canUpdate = await _authService.VerifyPermissionOfUser(currentUserId, "USER", "U");
            if (!canUpdate)
            {
                throw new UnauthorizedAccessException("Bạn không có quyền cập nhật thông tin người dùng");
            }
            var existAccount = await _unitOfWork.Repository<SysAccount>()
                                             .FirstOrDefaultAsync(r => r.IsDeleted != true && r.Id != accountId && req.UserName.Equals(r.UserName));
            if (existAccount != null)
            {
                throw new KeyExistsException(string.Format(CommonMessage.Message_Exists, "Account"));
            }

            var account = await _unitOfWork.Repository<SysAccount>().FirstOrDefaultAsync(x => x.IsDeleted != true && x.Id == accountId)
                                ?? throw new KeyNotFoundException(string.Format(CommonMessage.Message_NotFound, "Account"));
            List<IFormFile> lstAvatar = new List<IFormFile>();
            lstAvatar.Add(req.Avatar);
            var urlAvatar = await _fileService.AzureBlobUploadFiles(lstAvatar, currentUserId);
            account.UserName = req.UserName;
            account.Email = req.Email;
            account.PhoneNumber = req.PhoneNumber;
            account.FirstName = req.FirstName;
            account.LastName = req.LastName;
            account.Group = req.Group;
            account.Base = req.Base;
            account.Note = req.Note;
            account.Avatar = "";
            account.UpdatedDate = DateTime.Now;
            account.Updater = currentUserName;
            account.UpdatedById = currentUserId;
            _unitOfWork.Repository<SysAccount>().Update(account);
            var res = await _unitOfWork.SaveChangesAsync();

            return Utils.CreateResponseModel(res > 0);
        }

        public async Task<SysAccountResponse> GetUserById(Guid userId)
        {
            var detail = await _unitOfWork.Repository<SysAccount>()
               .Where(a => a.IsDeleted != true && a.Id == userId)
               .Select(r => new SysAccountResponse
               {
                   Id = r.Id,
                   Code = r.Code,
                   UserName = r.UserName,
                   Email = r.Email,
                   PhoneNumber = r.PhoneNumber,
                   FirstName = r.FirstName,
                   LastName = r.LastName,
                   Group = r.Group,
                   Base = r.Base,
                   Note = r.Note,
                   FullNameNoAccent = r.FullNameNoAccent,
                   Avatar = r.Avatar ?? "",
                   CreatedDate = r.CreatedDate,
                   Creator = r.CreatedName,
               }).FirstOrDefaultAsync()
               ?? throw new KeyNotFoundException(string.Format(CommonMessage.Message_NotFound, "Account")); ;

            return detail;
        }
    }
}