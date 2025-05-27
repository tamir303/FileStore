namespace FileStoreService.Features.Search.API.DTOs;

public class SearchResultDto
{
    public string FileId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public List<string>? Tags { get; set; }
    public string? UploadedBy { get; set; }
    public DateTime UploadedAt { get; set; }
    public bool IsPublic { get; set; }
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public double? RelevanceScore { get; set; }
    public List<string>? MatchedFields { get; set; }
}