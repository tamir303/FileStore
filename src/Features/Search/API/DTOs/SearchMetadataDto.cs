namespace FileStoreService.Features.Search.API.DTOs;

public class SearchMetadataDto
{
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public long TotalResults { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
    public TimeSpan SearchDuration { get; set; }
    public Dictionary<string, int> Facets { get; set; } = new();
}