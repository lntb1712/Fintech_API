using Microsoft.AspNetCore.Http;

namespace DomainService.Interfaces.File;

public interface IFileService
{
    Task<object> AzureBlobUploadFiles(List<IFormFile> files, Guid currentUserId);
}