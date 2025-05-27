using FileStoreService.Features.Storage.DTOs;
using FileStoreService.Shared.Enum;

namespace FileStoreService.Features.Storage.Application.Services;

/// <summary>
/// Service interface for file storage operations
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Stores a single file to the configured storage location
    /// </summary>
    /// <param name="fileStream">File stream to store</param>
    /// <param name="fileName">Name of the file</param>
    /// <param name="contentType">Content type of the file</param>
    /// <param name="metadata">Additional metadata for the file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Storage result with file path and metadata</returns>
    Task<FileStorageResult> StoreFileAsync(Stream fileStream, string fileName, string contentType, Dictionary<string, string>? metadata = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores multiple files in batch
    /// </summary>
    /// <param name="files">Collection of file storage requests</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Bulk storage result with individual file results</returns>
    Task<BulkStorageResult> StoreBulkFilesAsync(IEnumerable<FileStorageRequest> files, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a file stream from storage
    /// </summary>
    /// <param name="filePath">Path to the stored file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>File stream and metadata</returns>
    Task<FileRetrievalResult> GetFileStreamAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves file metadata without downloading the file
    /// </summary>
    /// <param name="filePath">Path to the stored file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>File metadata information</returns>
    Task<FileMetadata> GetFileMetadataAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a file from storage
    /// </summary>
    /// <param name="filePath">Path to the file to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if file was successfully deleted</returns>
    Task<bool> DeleteFileAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes multiple files in batch
    /// </summary>
    /// <param name="filePaths">Collection of file paths to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Bulk deletion result with individual results</returns>
    Task<BulkDeletionResult> DeleteBulkFilesAsync(IEnumerable<string> filePaths, CancellationToken cancellationToken = default);

    /// <summary>
    /// Moves a file to a different location within storage
    /// </summary>
    /// <param name="sourcePath">Current file path</param>
    /// <param name="destinationPath">New file path</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if file was successfully moved</returns>
    Task<bool> MoveFileAsync(string sourcePath, string destinationPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Copies a file to a different location within storage
    /// </summary>
    /// <param name="sourcePath">Source file path</param>
    /// <param name="destinationPath">Destination file path</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if file was successfully copied</returns>
    Task<bool> CopyFileAsync(string sourcePath, string destinationPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a file exists in storage
    /// </summary>
    /// <param name="filePath">Path to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if file exists</returns>
    Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the size of a stored file
    /// </summary>
    /// <param name="filePath">Path to the file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>File size in bytes, or null if file doesn't exist</returns>
    Task<long?> GetFileSizeAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a temporary download URL for a file (if supported by storage provider)
    /// </summary>
    /// <param name="filePath">Path to the file</param>
    /// <param name="expirationTime">URL expiration time</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Temporary download URL or null if not supported</returns>
    Task<string?> GenerateDownloadUrlAsync(string filePath, TimeSpan expirationTime, CancellationToken cancellationToken = default);

    /// <summary>
    /// Compresses a file using the specified compression algorithm
    /// </summary>
    /// <param name="sourceStream">Source file stream</param>
    /// <param name="compressionType">Type of compression to apply</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Compressed file stream</returns>
    Task<Stream> CompressFileAsync(Stream sourceStream, CompressionType compressionType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Decompresses a compressed file
    /// </summary>
    /// <param name="compressedStream">Compressed file stream</param>
    /// <param name="compressionType">Type of compression used</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Decompressed file stream</returns>
    Task<Stream> DecompressFileAsync(Stream compressedStream, CompressionType compressionType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates an archive containing multiple files
    /// </summary>
    /// <param name="files">Collection of files to include in archive</param>
    /// <param name="archiveFormat">Format of the archive (ZIP, TAR, etc.)</param>
    /// <param name="compressionLevel">Level of compression to apply</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Archive creation result with stream and metadata</returns>
    Task<ArchiveCreationResult> CreateArchiveAsync(IEnumerable<ArchiveFileRequest> files, ArchiveFormat archiveFormat, CompressionLevel compressionLevel, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets storage usage statistics
    /// </summary>
    /// <param name="path">Path to analyze (optional, analyzes entire storage if null)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Storage usage statistics</returns>
    Task<StorageUsageStats> GetStorageUsageAsync(string? path = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cleans up temporary files older than specified age
    /// </summary>
    /// <param name="maxAge">Maximum age of temporary files to keep</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cleanup result with number of files removed and space freed</returns>
    Task<CleanupResult> CleanupTemporaryFilesAsync(TimeSpan maxAge, CancellationToken cancellationToken = default);
}