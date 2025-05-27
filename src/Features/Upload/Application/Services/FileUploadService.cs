using FileStoreService.Features.Upload.API.DTOs;

namespace FileStoreService.Features.Upload.Application.Services;

public class FileUploadService : IFileUploadService
{
    public Task<UploadResponseDto> UploadSingleFileAsync(UploadRequestDto request, string userId, string? correlationId)
    {
        throw new NotImplementedException();
    }

    public Task<BulkUploadResponseDto> UploadBulkFilesAsync(BulkUploadRequestDto request, string userId, string? correlationId)
    {
        throw new NotImplementedException();
    }

    public Task<UploadProgressDto> GetUploadProgressAsync(string uploadId, string userId)
    {
        throw new NotImplementedException();
    }
}