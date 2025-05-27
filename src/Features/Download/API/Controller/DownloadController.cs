using System.Net;
using FileStoreService.Features.Download.API.DTOs;
using FileStoreService.Features.Download.Application.Services;
using FileStoreService.Shared.Constants;
using FileStoreService.Shared.DTOs;
using FileStoreService.Shared.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FileStoreService.Features.Download.API.Controller;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[ProducesResponseType(typeof(ErrorResponseDto), (int)HttpStatusCode.BadRequest)]
[ProducesResponseType(typeof(ErrorResponseDto), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseDto), (int)HttpStatusCode.InternalServerError)]
public class DownloadController : ControllerBase
{
    private readonly IFileDownloadService _downloadService;
    private readonly ILogger<DownloadController> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _cdnBaseUrl;

    public DownloadController(
        IFileDownloadService downloadService,
        ILogger<DownloadController> logger,
        IConfiguration configuration)
    {
        _downloadService = downloadService;
        _logger = logger;
        _configuration = configuration;
        _cdnBaseUrl = _configuration.GetValue<string>(FileConstants.ENV_CDN_BASE_URL, string.Empty);
    }

    [HttpGet("{fileId}")]
    [EnableRateLimiting(FileConstants.RATE_LIMIT_DOWNLOAD_POLICY)]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [ProducesResponseType((int)HttpStatusCode.PartialContent)]
    public async Task<IActionResult> DownloadFile(
        [FromRoute] string fileId,
        [FromQuery] bool forceDownload = false,
        [FromQuery] string? customFileName = null)
    {
        var correlationId = HttpContext.GetCorrelationId();
        var userId = User.GetUserId();
        var userAgent = Request.Headers.UserAgent.ToString();
        var ipAddress = HttpContext.GetClientIpAddress();
        var rangeHeader = Request.Headers.Range.ToString();

        Response.Headers[FileConstants.HEADER_CORRELATION_ID] = correlationId;

        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId ?? string.Empty,
            ["UserId"] = userId,
            ["FileId"] = fileId,
            ["Action"] = nameof(DownloadFile),
            ["ForceDownload"] = forceDownload,
            ["UserAgent"] = userAgent,
            ["IpAddress"] = ipAddress,
            ["RangeRequested"] = !string.IsNullOrEmpty(rangeHeader)
        });

        try
        {
            _logger.LogInformation("Starting file download for user {UserId}, file {FileId}", userId, fileId);

            var request = new DownloadRequestDto
            {
                FileId = fileId,
                ForceDownload = forceDownload,
                CustomFileName = customFileName
            };

            // Handle range requests for partial downloads
            if (!string.IsNullOrEmpty(rangeHeader))
            {
                var range = ParseRangeHeader(rangeHeader);
                request.RangeStart = range.Start;
                request.RangeEnd = range.End;
            }

            var result = await _downloadService.GetFileStreamAsync(request, userId, correlationId);

            if (!result.Success)
            {
                _logger.LogWarning("File download failed: {ErrorMessage}", result.ErrorMessage);
                return NotFound(new ErrorResponseDto
                {
                    Message = result.ErrorMessage ?? "File not found",
                    CorrelationId = correlationId ?? string.Empty
                });
            }

            // Set response headers
            Response.ContentType = result.ContentType;
            Response.Headers["Content-Length"] = result.FileSize.ToString();
            Response.Headers["Accept-Ranges"] = "bytes";
            Response.Headers["Last-Modified"] = DateTime.UtcNow.ToString("R");

            var fileName = customFileName ?? result.FileName;
            var contentDisposition = forceDownload ? "attachment" : "inline";
            Response.Headers.ContentDisposition = $"{contentDisposition}; filename=\"{fileName}\"";

            // Add additional headers if present
            if (result.Headers != null)
            {
                foreach (var header in result.Headers)
                {
                    Response.Headers[header.Key] = header.Value;
                }
            }

            _logger.LogInformation("File download initiated successfully for file {FileId}", fileId);

            // Return file stream (implementation depends on your file storage)
            return File(result.ContentType, fileName, enableRangeProcessing: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during file download for {FileId}", fileId);
            return StatusCode(500, new ErrorResponseDto
            {
                Message = "An unexpected error occurred during download",
                CorrelationId = correlationId ?? string.Empty
            });
        }
    }

    [HttpPost("bulk")]
    [EnableRateLimiting(FileConstants.RATE_LIMIT_DOWNLOAD_POLICY)]
    [ProducesResponseType(typeof(BulkDownloadResponseDto), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> DownloadBulk([FromBody] BulkDownloadRequestDto request)
    {
        var correlationId = HttpContext.GetCorrelationId();
        var userId = User.GetUserId();

        Response.Headers[FileConstants.HEADER_CORRELATION_ID] = correlationId;

        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId ?? string.Empty,
            ["UserId"] = userId,
            ["Action"] = nameof(DownloadBulk),
            ["FileCount"] = request.FileIds.Count,
            ["ArchiveFormat"] = request.Format.ToString()
        });

        try
        {
            _logger.LogInformation("Starting bulk download of {FileCount} files for user {UserId}",
                request.FileIds.Count, userId);

            if (request.FileIds.Count > FileConstants.DEFAULT_BULK_DOWNLOAD_LIMIT)
            {
                return BadRequest(new ErrorResponseDto
                {
                    Message = $"Cannot download more than {FileConstants.DEFAULT_BULK_DOWNLOAD_LIMIT} files at once",
                    CorrelationId = correlationId ?? string.Empty
                });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorResponseDto
                {
                    Message = "Invalid request data",
                    Errors = ModelState.GetErrors()!,
                    CorrelationId = correlationId ?? string.Empty
                });
            }

            var result = await _downloadService.CreateBulkDownloadAsync(request, userId, correlationId);

            if (!result.Success)
            {
                _logger.LogError("Bulk download creation failed: {ErrorMessage}", result.ErrorMessage);
                return BadRequest(new ErrorResponseDto
                {
                    Message = result.ErrorMessage ?? "Bulk download creation failed",
                    CorrelationId = correlationId ?? string.Empty,
                });
            }

            _logger.LogInformation("Bulk download archive created successfully: {ArchiveId}", result.ArchiveId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during bulk download creation");
            return StatusCode(500, new ErrorResponseDto
            {
                Message = "An unexpected error occurred during bulk download creation",
                CorrelationId = correlationId ?? string.Empty,
            });
        }
    }

    [HttpGet("stats/{fileId}")]
    [ResponseCache(Duration = 300)] // Cache for 5 minutes
    [ProducesResponseType(typeof(DownloadStatsDto), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetDownloadStats([FromRoute] string fileId)
    {
        var correlationId = HttpContext.GetCorrelationId();
        var userId = User.GetUserId();

        Response.Headers[FileConstants.HEADER_CORRELATION_ID] = correlationId;

        try
        {
            var stats = await _downloadService.GetDownloadStatsAsync(fileId, userId);

            if (stats == null)
            {
                return NotFound(new ErrorResponseDto
                {
                    Message = "File not found or access denied",
                    CorrelationId = correlationId ?? string.Empty
                });
            }

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving download stats for file {FileId}", fileId);
            return StatusCode(500, new ErrorResponseDto
            {
                Message = "Error retrieving download statistics",
                CorrelationId = correlationId ?? string.Empty
            });
        }
    }

    private (long? Start, long? End) ParseRangeHeader(string rangeHeader)
    {
        // Simple range header parsing - "bytes=start-end"
        if (!rangeHeader.StartsWith("bytes=")) return (null, null);
        var range = rangeHeader.Substring(6);
        var parts = range.Split('-');

        if (parts.Length != 2) return (null, null);
        long.TryParse(parts[0], out var start);
        long.TryParse(parts[1], out var end);

        return (start == 0 ? null : start, end == 0 ? null : end);
    }
}