using System.ComponentModel.DataAnnotations;
using FileStoreService.Shared.Enum;

namespace FileStoreService.Features.Download.API.DTOs;

public class BulkDownloadRequestDto
{
    [Required]
    [MinLength(1)]
    public List<string> FileIds { get; set; } = new();
        
    [StringLength(100)]
    public string? ArchiveName { get; set; }
        
    public ArchiveFormat Format { get; set; } = ArchiveFormat.Zip;
        
    public CompressionLevel Compression { get; set; } = CompressionLevel.Normal;
    
    public BulkDownloadRequestDto() { }
}