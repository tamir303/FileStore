using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace FileStoreService.Features.Upload.API.DTOs;

public class BulkUploadRequestDto
{
    [Required]
    [MinLength(1)]
    public List<IFormFile> Files { get; set; } = new();
        
    [StringLength(500)]
    public string? Description { get; set; }
        
    [StringLength(100)]
    public string? Category { get; set; }
        
    public Dictionary<string, string>? CustomProperties { get; set; }
        
    public List<string>? Tags { get; set; }
        
    public bool IsPublic { get; set; } = false;
        
    public DateTime? ExpiresAt { get; set; }
    
    public BulkUploadRequestDto() { }
}