using FileStoreService.Features.Storage.DTOs;
using FileStoreService.Shared.Enum;

namespace FileStoreService.Features.Storage.Application.Services;

public class LocalFileStorageService : IFileStorageService
{
    public Task<FileStorageResult> StoreFileAsync(Stream fileStream, string fileName, string contentType, Dictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<BulkStorageResult> StoreBulkFilesAsync(IEnumerable<FileStorageRequest> files, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<FileRetrievalResult> GetFileStreamAsync(string filePath, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<FileMetadata> GetFileMetadataAsync(string filePath, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<BulkDeletionResult> DeleteBulkFilesAsync(IEnumerable<string> filePaths, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> MoveFileAsync(string sourcePath, string destinationPath, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> CopyFileAsync(string sourcePath, string destinationPath, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<long?> GetFileSizeAsync(string filePath, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<string?> GenerateDownloadUrlAsync(string filePath, TimeSpan expirationTime, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Stream> CompressFileAsync(Stream sourceStream, CompressionType compressionType,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Stream> DecompressFileAsync(Stream compressedStream, CompressionType compressionType,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<ArchiveCreationResult> CreateArchiveAsync(IEnumerable<ArchiveFileRequest> files, ArchiveFormat archiveFormat, CompressionLevel compressionLevel,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<StorageUsageStats> GetStorageUsageAsync(string? path = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<CleanupResult> CleanupTemporaryFilesAsync(TimeSpan maxAge, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}