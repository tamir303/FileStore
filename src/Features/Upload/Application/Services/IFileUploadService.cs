using FileStoreService.Features.Upload.API.DTOs;

namespace FileStoreService.Features.Upload.Application.Services;

public interface IFileUploadService
{
    Task<UploadResponseDto> UploadSingleFileAsync(UploadRequestDto request, string userId, string? correlationId);
    Task<BulkUploadResponseDto> UploadBulkFilesAsync(BulkUploadRequestDto request, string userId, string? correlationId);
    Task<UploadProgressDto> GetUploadProgressAsync(string uploadId, string userId);
}