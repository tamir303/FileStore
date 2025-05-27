namespace FileStoreService.Infrastructure.Entities;

public class FileDocument
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
    public List<string> Tags { get; set; } = new();
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
    public int CurrentVersion { get; set; } = 1;
    public List<FileVersionDocument> Versions { get; set; } = new();
    public List<FilePermissionDocument> Permissions { get; set; } = new();
}