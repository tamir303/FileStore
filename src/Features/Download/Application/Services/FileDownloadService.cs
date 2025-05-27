using FileStoreService.Features.Download.API.DTOs;

namespace FileStoreService.Features.Download.Application.Services;

public class FileDownloadService : IFileDownloadService
{
    public Task<DownloadResponseDto> GetFileStreamAsync(DownloadRequestDto request, string userId, string? correlationId)
    {
        throw new NotImplementedException();
    }

    public Task<BulkDownloadResponseDto> CreateBulkDownloadAsync(BulkDownloadRequestDto request, string userId, string? correlationId)
    {
        throw new NotImplementedException();
    }

    public Task<DownloadStatsDto> GetDownloadStatsAsync(string fileId, string userId)
    {
        throw new NotImplementedException();
    }
}