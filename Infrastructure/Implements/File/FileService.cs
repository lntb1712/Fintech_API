using System.Net;
using System.Net.Security;
using Azure.Core.Pipeline;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Common.Settings;
using Common.UnitOfWork.UnitOfWorkPattern;
using Common.Utils;
using DomainService.Interfaces.File;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Model.ResponseModel.File;

namespace Infrastructure.Implements.File;

public class FileService(IUnitOfWork unitOfWork, IMemoryCache memoryCache)
    : BaseService(unitOfWork, memoryCache), IFileService
{
    private static readonly List<string> _extensionWhitelist =
        new string[] { ".png", ".jpg", ".jpeg", ".gif", ".pdf", ".doc", ".docx", ".xlsx" }.ToList();
    private static readonly string[] jpgImage = new string[] { "jpg", "jpeg" };
    private AppSettings _appSetting;
    
    
    public async Task<object> AzureBlobUploadFiles(List<IFormFile> files, Guid currentUserId)
    {
        // var permission = _userService.GetPermission(currentUserId, PermissionConstant.ACTIVITY_NEWS_EDITOR, ActivityPermissionType.Create);
        // if (!permission) throw new AppException(CommonMessage.Message_NotHavePermissionUseFunction);

        var responses = new List<AzureBlobUploadFilesResponse>();

        foreach (var file in files)
        {
            if (file == null || file.Length == 0) continue;

            if (file.Length > 5 * 1024 * 1024)
                throw new Exception($"Tệp \"{file.FileName}\" vượt quá dung lượng cho phép (tối đa 5MB).");

            // Gọi lại hàm AzureBlobUploadFile
            using (var stream = file.OpenReadStream())
            {
                var result = await AzureBlobUploadFile(stream, file.FileName, "uploads");
                responses.Add(new AzureBlobUploadFilesResponse
                {
                    FileName = result.FileName,
                    FileNameOnStorage = result.FileNameOnStorage,
                    DownloadPath = result.DownloadPath,
                    AbsolutePath = result.AbsolutePath,
                });
            }
        }

        return Utils.CreateResponseModel(responses, responses.Count);
    }

    private async Task<CabinbookAttachmentResponse> AzureBlobUploadFile(Stream fileStream, string originFileName,
        string module)
    {
        var originFileExtension = Path.GetExtension(originFileName).ToLower();
        ValidateFileTypeUpload(originFileExtension);

        //Set not require http
        ServicePointManager.Expect100Continue = true;
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        ServicePointManager.ServerCertificateValidationCallback =
            new RemoteCertificateValidationCallback(delegate { return true; });
        Utils.EnableTrustedHosts();

        //Whatever options class suitable for your client
        BlobContainerClient container = new(_appSetting.AzureBlobConnection, _appSetting.AzureBlobContainerName,
            new BlobClientOptions
            {
                Transport = new HttpClientTransport(new HttpClient { Timeout = Timeout.InfiniteTimeSpan }),
                Retry = { NetworkTimeout = Timeout.InfiniteTimeSpan }
            });
        container.CreateIfNotExists();

        string saveFileName = $"{Guid.NewGuid()}{originFileExtension}";
        var storageBlobRootFolder =
            $"{_appSetting.AzureBlobRootFolder}/{module}/{DateTime.Now:yyyy}/{DateTime.Now:MM}/{DateTime.Now:dd}";
        var newPathUpload = $"{storageBlobRootFolder}/{saveFileName}";

        var blobUploadOptions = new BlobUploadOptions();
        var blobHttpHeader = new BlobHttpHeaders();
        string contentType = AzureBlobGetContentType(originFileExtension);
        blobHttpHeader.ContentType = contentType;
        blobUploadOptions.HttpHeaders = blobHttpHeader;

        BlobClient blobClient = container.GetBlobClient(newPathUpload);
        await blobClient.UploadAsync(fileStream, blobUploadOptions);

        //Return Absolute Path
        return new CabinbookAttachmentResponse
        {
            FileName = originFileName, //File real of user
            FileNameOnStorage = saveFileName, //File alias on storage/server
            DownloadPath = $"{_appSetting.AzureBlobUrl}{blobClient.Uri.AbsolutePath}",
            AbsolutePath = $"{blobClient.Uri.AbsolutePath}",
        };
    }

    private static void ValidateFileTypeUpload(string ext)
    {
        if (string.IsNullOrWhiteSpace(ext) || (!string.IsNullOrWhiteSpace(ext) &&
                                               !_extensionWhitelist.Any(p =>
                                                   p.Equals(ext, StringComparison.OrdinalIgnoreCase))))
            throw new Exception($"File type is not allowed");
    }
    
    private static string AzureBlobGetContentType(string originFileExtension)
    {
        if (jpgImage.Any(originFileExtension.EndsWith))
            return "image/jpeg";
        else if (originFileExtension.EndsWith("png"))
            return "image/png";
        else if (originFileExtension.EndsWith("mp4"))
            return "video/mp4";
        else if (originFileExtension.EndsWith("pdf"))
            return "application/pdf";
        else if (originFileExtension.EndsWith("doc") || originFileExtension.EndsWith("docx"))
            return "application/msword";
        else if (originFileExtension.EndsWith("xls") || originFileExtension.EndsWith("xlsx"))
            return "application/vnd.ms-excel";
        else if (originFileExtension.EndsWith("ppt") || originFileExtension.EndsWith("pptx"))
            return "application/vnd.ms-powerpoint";
        else
            return "application/octet-stream";
    }

}