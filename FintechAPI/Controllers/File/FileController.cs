using Controllers;
using DomainService.Interfaces.File;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API_Template.Controllers.File
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : BaseController
    {
        private readonly IFileService _fileService;

        public FileController(IHttpContextAccessor httpContextAccessor, IFileService fileService) : base(httpContextAccessor)
        {
            _fileService = fileService;
        }
        
        [HttpPost("upload-files")]
        public async Task<IActionResult> UploadFiles([FromForm] List<IFormFile> files)
        {
            var result = await _fileService.AzureBlobUploadFiles(files, currentUserId);
            return Ok(result);
        }
    }
}
