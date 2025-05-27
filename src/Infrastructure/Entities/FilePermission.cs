using System.ComponentModel.DataAnnotations.Schema;

namespace FileStoreService.Infrastructure.Entities;

[Table("FilePermission")]
public class FilePermission
{
    public Guid Id { get; set; }
    public Guid FileId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Permission { get; set; } = string.Empty; // "read", "write", "delete", etc.
    public DateTime GrantedAt { get; set; }
    public string GrantedBy { get; set; } = string.Empty;

    // Navigation property
    public virtual FileMetadata FileMetadata { get; set; } = null!;
}