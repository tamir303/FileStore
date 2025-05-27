using FileStoreService.Features.Download.API.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace FileStoreService.Features.Download.Application.Services;

public interface IFileDownloadService
{
    Task<DownloadResponseDto> GetFileStreamAsync(DownloadRequestDto request, string userId, string? correlationId);
    Task<BulkDownloadResponseDto> CreateBulkDownloadAsync(BulkDownloadRequestDto request, string userId, string? correlationId);
    Task<DownloadStatsDto> GetDownloadStatsAsync(string fileId, string userId);
}