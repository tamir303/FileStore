using FileStoreService.Features.Search.API.DTOs;

namespace FileStoreService.Features.Search.Application.Services;

public class FileSearchService : IFileSearchService
{
    public Task<SearchResponseDto> SearchFilesAsync(SearchRequestDto request, string userId, string? correlationId)
    {
        throw new NotImplementedException();
    }

    public Task<SearchResponseDto> AdvancedSearchAsync(AdvancedSearchRequestDto request, string userId, string? correlationId)
    {
        throw new NotImplementedException();
    }

    public Task<SearchSuggestionsDto> GetSearchSuggestionsAsync(string? query, string userId)
    {
        throw new NotImplementedException();
    }

    public Task<Dictionary<string, int>> GetSearchFacetsAsync(string userId)
    {
        throw new NotImplementedException();
    }
}