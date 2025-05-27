using System.ComponentModel.DataAnnotations.Schema;

namespace FileStoreService.Infrastructure.Entities;

[Table("FileMetadata")]
public class FileMetadata
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string UploadedBy { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public string? Category { get; set; }
    public string? Description { get; set; }
    public string? Tags { get; set; }
    [NotMapped]
    public Dictionary<string, string>? CustomMetadata { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
    public int CurrentVersion { get; set; } = 1;

    // Navigation properties
    public virtual ICollection<FileVersion> Versions { get; set; } = new List<FileVersion>();
    public virtual ICollection<FilePermission> Permissions { get; set; } = new List<FilePermission>();
}