namespace FileStoreService.Features.Storage.DTOs;

/// <summary>
/// File metadata information
/// </summary>
public class FileMetadata
{
    /// <summary>Primary key</summary>
    public Guid Id { get; set; }

    /// <summary>Name of the file (max 255)</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>MIME type (max 100)</summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>Absolute or relative path on disk (max 500)</summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>User who uploaded (max 100)</summary>
    public string UploadedBy { get; set; } = string.Empty;

    /// <summary>Optional category (max 50)</summary>
    public string? Category { get; set; }

    /// <summary>Optional description (max 1000)</summary>
    public string? Description { get; set; }

    /// <summary>Comma-separated tags, or you can deserialize into a List&lt;string&gt;</summary>
    public List<string>? Tags { get; set; }

    /// <summary>When the file was first stored</summary>
    public DateTime UploadedAt { get; set; }

    /// <summary>Size in bytes</summary>
    public long FileSize { get; set; }

    /// <summary>When the file record was created</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>When the file record was last modified</summary>
    public DateTime LastModified { get; set; }

    /// <summary>Optional checksum (e.g. MD5, SHA256)</summary>
    public string? Checksum { get; set; }

    /// <summary>Arbitrary key/value metadata</summary>
    public Dictionary<string, string>? CustomMetadata { get; set; }
}