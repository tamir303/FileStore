using System.Security.Authentication;
using FileStoreService.Etc.Models;
using FileStoreService.Features.Validate.DTOs;
using Microsoft.AspNetCore.Http;

namespace FileStoreService.Features.Validate.Application.Services;

/// <summary>
/// Service interface for file validation operations
/// </summary>
public interface IFileValidationService
{
    /// <summary>
    /// Validates a single file upload request
    /// </summary>
    /// <param name="file">The file to validate</param>
    /// <param name="settings">File storage settings for validation rules</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation result with details</returns>
    Task<FileValidationResult> ValidateFileAsync(IFormFile file, FileStorageSettings settings, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates multiple files for bulk upload
    /// </summary>
    /// <param name="files">Collection of files to validate</param>
    /// <param name="settings">File storage settings for validation rules</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Bulk validation result with individual file results</returns>
    Task<BulkFileValidationResult> ValidateBulkFilesAsync(IEnumerable<IFormFile> files, FileStorageSettings settings, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates file content type against allowed types
    /// </summary>
    /// <param name="contentType">Content type to validate</param>
    /// <param name="allowedTypes">List of allowed content types</param>
    /// <returns>True if content type is allowed</returns>
    bool IsContentTypeAllowed(string contentType, IEnumerable<string> allowedTypes);

    /// <summary>
    /// Validates file size against configured limits
    /// </summary>
    /// <param name="fileSize">Size of the file in bytes</param>
    /// <param name="maxSizeBytes">Maximum allowed size in bytes</param>
    /// <returns>True if file size is within limits</returns>
    bool IsFileSizeValid(long fileSize, long maxSizeBytes);

    /// <summary>
    /// Validates total size for bulk uploads
    /// </summary>
    /// <param name="totalSize">Total size of all files in bytes</param>
    /// <param name="maxBulkSizeBytes">Maximum allowed bulk size in bytes</param>
    /// <returns>True if total size is within limits</returns>
    bool IsBulkSizeValid(long totalSize, long maxBulkSizeBytes);

    /// <summary>
    /// Validates file name for security and format compliance
    /// </summary>
    /// <param name="fileName">File name to validate</param>
    /// <returns>Validation result with sanitized file name if needed</returns>
    FileNameValidationResult ValidateFileName(string fileName);

    /// <summary>
    /// Scans file content for malware (if virus scanning is enabled)
    /// </summary>
    /// <param name="fileStream">File stream to scan</param>
    /// <param name="fileName">Name of the file being scanned</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Scan result indicating if file is safe</returns>
    Task<MalwareScanResult> ScanForMalwareAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates file signature/magic bytes to ensure file integrity
    /// </summary>
    /// <param name="fileStream">File stream to validate</param>
    /// <param name="expectedContentType">Expected content type based on file extension</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if file signature matches expected content type</returns>
    Task<bool> ValidateFileSignatureAsync(Stream fileStream, string expectedContentType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates file checksum for integrity verification
    /// </summary>
    /// <param name="fileStream">File stream to calculate checksum for</param>
    /// <param name="algorithm">Hash algorithm to use (MD5, SHA256, etc.)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Calculated checksum as hex string</returns>
    Task<string> CalculateChecksumAsync(Stream fileStream, HashAlgorithmType algorithm = HashAlgorithmType.Sha256, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates custom metadata properties
    /// </summary>
    /// <param name="metadata">Custom metadata dictionary</param>
    /// <param name="maxProperties">Maximum number of allowed properties</param>
    /// <param name="maxKeyLength">Maximum length of property keys</param>
    /// <param name="maxValueLength">Maximum length of property values</param>
    /// <returns>Validation result for metadata</returns>
    MetadataValidationResult ValidateMetadata(Dictionary<string, string>? metadata, int maxProperties = 50, int maxKeyLength = 100, int maxValueLength = 500);
}