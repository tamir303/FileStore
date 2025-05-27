using System.ComponentModel.DataAnnotations;
using FileStoreService.Shared.Enum;

namespace FileStoreService.Features.Search.API.DTOs;

public class SearchRequestDto
{
    [StringLength(200)]
    public string? Query { get; set; }
        
    [StringLength(100)]
    public string? FileName { get; set; }
        
    public List<string>? ContentTypes { get; set; }
        
    public List<string>? Categories { get; set; }
        
    public List<string>? Tags { get; set; }
        
    public long? MinFileSize { get; set; }
        
    public long? MaxFileSize { get; set; }
        
    public DateTime? UploadedAfter { get; set; }
        
    public DateTime? UploadedBefore { get; set; }
        
    public string? UploadedBy { get; set; }
        
    public bool? IsPublic { get; set; }
        
    public bool IncludeDeleted { get; set; } = false;
        
    // Pagination
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;
        
    [Range(1, 100)]
    public int PageSize { get; set; } = 20;
        
    // Sorting
    public SortBy SortBy { get; set; } = SortBy.UploadedAt;
        
    public SortOrder SortOrder { get; set; } = SortOrder.Descending;

    // Advanced search options
    public SearchMode SearchMode { get; set; } = SearchMode.Contains;
        
    public bool CaseSensitive { get; set; } = false;
}