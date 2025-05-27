using System.Net;
using FileStoreService.Features.Upload.API.DTOs;
using FileStoreService.Features.Upload.Application.Services;
using FileStoreService.Shared.Constants;
using FileStoreService.Shared.DTOs;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FileStoreService.Shared.Extensions;

namespace FileStoreService.Features.Upload.API.Controller;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[ProducesResponseType(typeof(ErrorResponseDto), (int)HttpStatusCode.BadRequest)]
[ProducesResponseType(typeof(ErrorResponseDto), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseDto), (int)HttpStatusCode.InternalServerError)]
public class UploadController : ControllerBase
{
    private readonly IFileUploadService _uploadService;
    private readonly ILogger<UploadController> _logger;
    private readonly IConfiguration _configuration;
    private readonly long _maxSingleFileSize;
    private readonly long _maxBulkUploadSize;

    public UploadController(
        IFileUploadService uploadService,
        ILogger<UploadController> logger,
        IConfiguration configuration)
    {
        _uploadService = uploadService;
        _logger = logger;
        _configuration = configuration;

        // Get size limits from environment or use defaults
        _maxSingleFileSize =
            _configuration.GetValue<long>(FileConstants.ENV_MAX_FILE_SIZE, FileConstants.SINGLE_FILE_SIZE_LIMIT);
        _maxBulkUploadSize =
            _configuration.GetValue<long>(FileConstants.ENV_MAX_BULK_SIZE, FileConstants.BULK_UPLOAD_SIZE_LIMIT);
    }

    [HttpPost("single")]
    [EnableRateLimiting(FileConstants.RATE_LIMIT_UPLOAD_POLICY)]
    [RequestSizeLimit(FileConstants.SINGLE_FILE_SIZE_LIMIT)]
    [RequestFormLimits(MultipartBodyLengthLimit = FileConstants.SINGLE_FILE_SIZE_LIMIT)]
    [ProducesResponseType(typeof(UploadResponseDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), (int)HttpStatusCode.BadGateway)]
    public async Task<IActionResult> UploadSingle([FromForm] UploadRequestDto request)
    {
        var correlationId = HttpContext.GetCorrelationId();
        var userId = User.GetUserId();
        var userAgent = Request.Headers.UserAgent.ToString();
        var ipAddress = HttpContext.GetClientIpAddress();

        // Add correlation ID to response headers
        Response.Headers[FileConstants.HEADER_CORRELATION_ID] = correlationId;
        Response.Headers[FileConstants.HEADER_API_VERSION] = "1.0";

        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId ?? string.Empty,
            ["UserId"] = userId,
            ["Action"] = nameof(UploadSingle),
            ["FileName"] = request.File.FileName,
            ["FileSize"] = request.File.Length,
            ["ContentType"] = request.File.ContentType,
            ["UserAgent"] = userAgent,
            ["IpAddress"] = ipAddress
        });

        try
        {
            _logger.LogInformation("Starting single file upload for user {UserId}", userId);

            // Validate file size against configured limit
            if (request.File.Length > _maxSingleFileSize)
            {
                _logger.LogWarning("File size {FileSize} exceeds limit {MaxSize}", request.File.Length,
                    _maxSingleFileSize);
                return StatusCode(413, new ErrorResponseDto
                {
                    Message = $"File size exceeds maximum allowed size of {_maxSingleFileSize / (1024 * 1024)}MB",
                    CorrelationId = correlationId ?? string.Empty
                });
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for upload request");
                return BadRequest(new ErrorResponseDto
                {
                    Message = "Invalid request data",
                    Errors = ModelState.GetErrors()!,
                    CorrelationId = correlationId ?? string.Empty
                });
            }

            var result = await _uploadService.UploadSingleFileAsync(request, userId, correlationId);

            if (result.Success)
            {
                _logger.LogInformation("File uploaded successfully with ID {FileId}", result.FileId);
                return Ok(result);
            }

            _logger.LogError("File upload failed: {ErrorMessage}", result.ErrorMessage);
            return BadRequest(new ErrorResponseDto
            {
                Message = result.ErrorMessage ?? "Upload failed",
                CorrelationId = correlationId ?? string.Empty
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during file upload");
            return StatusCode(500, new ErrorResponseDto
            {
                Message = "An unexpected error occurred during upload",
                CorrelationId = correlationId ?? string.Empty
            });
        }
    }

    [HttpPost("bulk")]
    [EnableRateLimiting(FileConstants.RATE_LIMIT_BULK_UPLOAD_POLICY)]
    [RequestSizeLimit(FileConstants.BULK_UPLOAD_SIZE_LIMIT)]
    [RequestFormLimits(MultipartBodyLengthLimit = FileConstants.BULK_UPLOAD_SIZE_LIMIT)]
    [ProducesResponseType(typeof(BulkUploadResponseDto), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> UploadBulk([FromForm] BulkUploadRequestDto request)
    {
        var correlationId = HttpContext.GetCorrelationId();
        var userId = User.GetUserId();

        Response.Headers[FileConstants.HEADER_CORRELATION_ID] = correlationId;
        Response.Headers[FileConstants.HEADER_API_VERSION] = "1.0";

        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId ?? string.Empty,
            ["UserId"] = userId,
            ["Action"] = nameof(UploadBulk),
            ["FileCount"] = request.Files.Count,
            ["TotalSize"] = request.Files.Sum(f => f.Length)
        });

        try
        {
            _logger.LogInformation("Starting bulk upload of {FileCount} files for user {UserId}",
                request.Files.Count, userId);

            var totalSize = request.Files.Sum(f => f.Length);
            if (totalSize > _maxBulkUploadSize)
            {
                _logger.LogWarning("Total upload size {TotalSize} exceeds bulk limit {MaxSize}",
                    totalSize, _maxBulkUploadSize);
                return StatusCode(413, new ErrorResponseDto
                {
                    Message =
                        $"Total upload size exceeds maximum allowed size of {_maxBulkUploadSize / (1024 * 1024)}MB",
                    CorrelationId = correlationId ?? string.Empty
                });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorResponseDto
                {
                    Message = "Invalid request data",
                    Errors = ModelState.GetErrors()!,
                    CorrelationId = correlationId ?? string.Empty,
                });
            }

            var result = await _uploadService.UploadBulkFilesAsync(request, userId, correlationId);

            _logger.LogInformation("Bulk upload completed: {Successful}/{Total} files successful",
                result.SuccessfulUploads, result.TotalFiles);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during bulk upload");
            return StatusCode(500, new ErrorResponseDto
            {
                Message = "An unexpected error occurred during bulk upload",
                CorrelationId = correlationId ?? string.Empty,
            });
        }
    }

    [HttpGet("progress/{uploadId}")]
    [ProducesResponseType(typeof(UploadProgressDto), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> GetUploadProgress([FromRoute] string uploadId)
    {
        var correlationId = HttpContext.GetCorrelationId();
        var userId = User.GetUserId();

        Response.Headers[FileConstants.HEADER_CORRELATION_ID] = correlationId;

        try
        {
            var progress = await _uploadService.GetUploadProgressAsync(uploadId, userId);

            if (progress == null)
            {
                _logger.LogWarning("Upload progress not found for ID {UploadId}", uploadId);
                return NotFound(new ErrorResponseDto
                {
                    Message = "Upload progress not found",
                    CorrelationId = correlationId ?? string.Empty
                });
            }

            return Ok(progress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving upload progress for {UploadId}", uploadId);
            return StatusCode(500, new ErrorResponseDto
            {
                Message = "Error retrieving upload progress",
                CorrelationId = correlationId ?? string.Empty
            });
        }
    }
}