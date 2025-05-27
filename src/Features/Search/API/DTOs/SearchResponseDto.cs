namespace FileStoreService.Features.Search.API.DTOs;

public class SearchResponseDto
{
    public List<SearchResultDto> Results { get; set; } = new();
    public SearchMetadataDto Metadata { get; set; } = new();
}