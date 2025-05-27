using System.ComponentModel.DataAnnotations.Schema;

namespace FileStoreService.Infrastructure.Entities;

[Table("FileVersion")]
public class FileVersion
{
    public Guid Id { get; set; }
    public Guid FileId { get; set; }
    public int VersionNumber { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string UploadedBy { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public string? ChangeDescription { get; set; }

    // Navigation property
    public virtual FileMetadata FileMetadata { get; set; } = null!;
}