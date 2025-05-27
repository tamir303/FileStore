using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace FileStoreService.Features.Upload.API.DTOs;

public class UploadRequestDto
{
    [Required]
    public required IFormFile File { get; set; }
        
    [StringLength(500)]
    public string? Description { get; set; }
        
    [StringLength(100)]
    public string? Category { get; set; }
        
    public Dictionary<string, string>? CustomProperties { get; set; }
        
    public List<string>? Tags { get; set; }
        
    public bool IsPublic { get; set; } = false;
        
    public DateTime? ExpiresAt { get; set; }
    
    public UploadRequestDto() { }
}