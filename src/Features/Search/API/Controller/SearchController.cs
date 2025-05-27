using System.Net;
using FileStoreService.Features.Search.API.DTOs;
using FileStoreService.Features.Search.Application.Services;
using FileStoreService.Shared.Constants;
using FileStoreService.Shared.DTOs;
using FileStoreService.Shared.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Memory;

namespace FileStoreService.Features.Search.API.Controller;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class SearchController : ControllerBase
{
    private readonly IFileSearchService _searchService;
    private readonly ILogger<SearchController> _logger;
    private readonly IMemoryCache _cache;

    public SearchController(
        IFileSearchService searchService,
        ILogger<SearchController> logger,
        IMemoryCache cache)
    {
        _searchService = searchService;
        _logger = logger;
        _cache = cache;
    }

    [HttpPost]
    [EnableRateLimiting(FileConstants.RATE_LIMIT_SEARCH_POLICY)]
    [ProducesResponseType(typeof(SearchResponseDto), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> Search([FromBody] SearchRequestDto request)
    {
        var correlationId = HttpContext.GetCorrelationId();
        var userId = User.GetUserId();

        Response.Headers[FileConstants.HEADER_CORRELATION_ID] = correlationId;

        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId ?? string.Empty,
            ["UserId"] = userId,
            ["Action"] = nameof(Search),
            ["Query"] = request.Query ?? "N/A",
            ["Page"] = request.Page,
            ["PageSize"] = request.PageSize
        });

        try
        {
            _logger.LogInformation("Starting file search for user {UserId} with query: {Query}",
                userId, request.Query);

            // Validate page size
            if (request.PageSize > FileConstants.MAX_SEARCH_PAGE_SIZE)
            {
                request.PageSize = FileConstants.MAX_SEARCH_PAGE_SIZE;
                _logger.LogInformation("Page size adjusted to maximum allowed: {MaxPageSize}",
                    FileConstants.MAX_SEARCH_PAGE_SIZE);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorResponseDto
                {
                    Message = "Invalid search parameters",
                    Errors = ModelState.GetErrors()!,
                    CorrelationId = correlationId ?? string.Empty
                });
            }

            var result = await _searchService.SearchFilesAsync(request, userId, correlationId);

            _logger.LogInformation("Search completed: {ResultCount} results found in {Duration}ms",
                result.Metadata.TotalResults, result.Metadata.SearchDuration.TotalMilliseconds);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during file search");
            return StatusCode(500, new ErrorResponseDto
            {
                Message = "An unexpected error occurred during search",
                CorrelationId = correlationId ?? string.Empty
            });
        }
    }

    [HttpPost("advanced")]
    [EnableRateLimiting(FileConstants.RATE_LIMIT_SEARCH_POLICY)]
    [ProducesResponseType(typeof(SearchResponseDto), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> AdvancedSearch([FromBody] AdvancedSearchRequestDto request)
    {
        var correlationId = HttpContext.GetCorrelationId();
        var userId = User.GetUserId();

        Response.Headers[FileConstants.HEADER_CORRELATION_ID] = correlationId;

        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId ?? string.Empty,
            ["UserId"] = userId,
            ["Action"] = nameof(AdvancedSearch),
            ["ConditionCount"] = request.Conditions?.Count ?? 0
        });

        try
        {
            _logger.LogInformation("Starting advanced file search for user {UserId} with {ConditionCount} conditions",
                userId, request.Conditions?.Count ?? 0);

            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorResponseDto
                {
                    Message = "Invalid advanced search parameters",
                    Errors = ModelState.GetErrors()!,
                    CorrelationId = correlationId ?? string.Empty
                });
            }

            var result = await _searchService.AdvancedSearchAsync(request, userId, correlationId);

            _logger.LogInformation("Advanced search completed: {ResultCount} results found",
                result.Metadata.TotalResults);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during advanced search");
            return StatusCode(500, new ErrorResponseDto
            {
                Message = "An unexpected error occurred during advanced search",
                CorrelationId = correlationId ?? string.Empty
            });
        }
    }

    [HttpGet("suggestions")]
    [ResponseCache(Duration = 1800)] // Cache for 30 minutes
    [ProducesResponseType(typeof(SearchSuggestionsDto), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetSearchSuggestions([FromQuery] string? query = null)
    {
        var correlationId = HttpContext.GetCorrelationId();
        var userId = User.GetUserId();
        var cacheKey = $"{FileConstants.CACHE_KEY_SEARCH_SUGGESTIONS}_{userId}_{query}";

        Response.Headers[FileConstants.HEADER_CORRELATION_ID] = correlationId;

        try
        {
            // Try to get from cache first
            if (_cache.TryGetValue(cacheKey, out SearchSuggestionsDto cachedSuggestions))
            {
                _logger.LogDebug("Returning cached search suggestions for user {UserId}", userId);
                return Ok(cachedSuggestions);
            }

            var suggestions = await _searchService.GetSearchSuggestionsAsync(query, userId);

            // Cache the results
            _cache.Set(cacheKey, suggestions, TimeSpan.FromMinutes(30));

            _logger.LogInformation("Search suggestions generated for user {UserId}", userId);
            return Ok(suggestions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating search suggestions");
            return StatusCode(500, new ErrorResponseDto
            {
                Message = "Error generating search suggestions",
                CorrelationId = correlationId ?? string.Empty
            });
        }
    }

    [HttpGet("facets")]
    [ResponseCache(Duration = 900)] // Cache for 15 minutes
    [ProducesResponseType(typeof(Dictionary<string, int>), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetSearchFacets()
    {
        var correlationId = HttpContext.GetCorrelationId();
        var userId = User.GetUserId();

        Response.Headers[FileConstants.HEADER_CORRELATION_ID] = correlationId;

        try
        {
            var facets = await _searchService.GetSearchFacetsAsync(userId);

            _logger.LogInformation("Search facets retrieved for user {UserId}", userId);
            return Ok(facets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving search facets");
            return StatusCode(500, new ErrorResponseDto
            {
                Message = "Error retrieving search facets",
                CorrelationId = correlationId ?? string.Empty
            });
        }
    }
}