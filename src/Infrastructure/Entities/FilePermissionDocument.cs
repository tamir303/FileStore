namespace FileStoreService.Infrastructure.Entities;

public class FilePermissionDocument
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Permission { get; set; } = string.Empty;
    public DateTime GrantedAt { get; set; }
    public string GrantedBy { get; set; } = string.Empty;
}