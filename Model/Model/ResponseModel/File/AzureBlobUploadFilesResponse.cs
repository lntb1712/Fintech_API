namespace Model.ResponseModel.File;

public class AzureBlobUploadFilesResponse
{
    public string FileName { get; set; }
    public string FileNameOnStorage { get; set; }
    public string DownloadPath { get; set; }
    public string AbsolutePath { get; set; }
}

public class CabinbookAttachmentResponse : CabinbookAttachmentRequest
{
    public string DownloadPath { get; set; } = string.Empty;
    public string ThumbnailPath { get; set; } = string.Empty;
    public string? FileNameOnStorage { get; set; }
    public string? AbsolutePath { get; set; }
    public Guid RefId { get; set; }
}

public class CabinbookCommonAttachmentResponse : CabinbookCommonAttachmentRequest
{
}

public class CabinbookCommonAttachmentMobileResponse
{
    public int GroupID { get; set; }
    public int FileID { get; set; }
    public string? DownloadPath { get; set; }
    public string? ThumbnailPath { get; set; }
    public string? OriginalFileName { get; set; }
    public string? ContentBase64 { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsDeleted { get; set; }
    public Guid RefId { get; set; }
}

public class CabinbookAttachmentRequest
{
    public int? GroupId { get; set; }
    public int? FileID { get; set; }
    public string FileName { get; set; } = string.Empty;
}

public class CabinbookCommonUploadAttachmentRequest
{
    public string? OriginalFileName { get; set; }
    public string? ContentBase64 { get; set; }
}

public class CabinbookCommonAttachmentRequest
{
    public int GroupId { get; set; }

    public int FileID { get; set; }

    public string? OriginalFileName { get; set; }

    public byte[]? FoFileSource { get; set; }

    public string? FoFileUrl { get; set; }

    public int? DisplayOrder { get; set; }

    public string? FilePath { get; set; }

    public bool? IsDeleted { get; set; }

    public string? ThumbnailPath { get; set; }
    public Guid? RefId { get; set; }
}
