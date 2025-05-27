namespace FileStoreService.Infrastructure.Entities;

public class FileVersionDocument
{
    public Guid Id { get; set; }
    public int VersionNumber { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string UploadedBy { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public string? ChangeDescription { get; set; }
}