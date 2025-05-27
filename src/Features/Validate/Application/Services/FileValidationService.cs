using System.Security.Authentication;
using FileStoreService.Etc.Models;
using FileStoreService.Features.Validate.DTOs;
using Microsoft.AspNetCore.Http;

namespace FileStoreService.Features.Validate.Application.Services;

public class FileValidationService : IFileValidationService
{
    public Task<FileValidationResult> ValidateFileAsync(IFormFile file, FileStorageSettings settings, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<BulkFileValidationResult> ValidateBulkFilesAsync(IEnumerable<IFormFile> files, FileStorageSettings settings,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public bool IsContentTypeAllowed(string contentType, IEnumerable<string> allowedTypes)
    {
        throw new NotImplementedException();
    }

    public bool IsFileSizeValid(long fileSize, long maxSizeBytes)
    {
        throw new NotImplementedException();
    }

    public bool IsBulkSizeValid(long totalSize, long maxBulkSizeBytes)
    {
        throw new NotImplementedException();
    }

    public FileNameValidationResult ValidateFileName(string fileName)
    {
        throw new NotImplementedException();
    }

    public Task<MalwareScanResult> ScanForMalwareAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ValidateFileSignatureAsync(Stream fileStream, string expectedContentType,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<string> CalculateChecksumAsync(Stream fileStream, HashAlgorithmType algorithm,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public MetadataValidationResult ValidateMetadata(Dictionary<string, string>? metadata, int maxProperties = 50, int maxKeyLength = 100,
        int maxValueLength = 500)
    {
        throw new NotImplementedException();
    }
}