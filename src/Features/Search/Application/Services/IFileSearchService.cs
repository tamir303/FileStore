using FileStoreService.Features.Search.API.DTOs;

namespace FileStoreService.Features.Search.Application.Services;

public interface IFileSearchService
{
    Task<SearchResponseDto> SearchFilesAsync(SearchRequestDto request, string userId, string? correlationId);
    Task<SearchResponseDto> AdvancedSearchAsync(AdvancedSearchRequestDto request, string userId, string? correlationId);
    Task<SearchSuggestionsDto> GetSearchSuggestionsAsync(string? query, string userId);
    Task<Dictionary<string, int>> GetSearchFacetsAsync(string userId);
}