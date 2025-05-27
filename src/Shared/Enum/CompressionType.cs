namespace FileStoreService.Shared.Enum;

/// <summary>
/// Supported compression types
/// </summary>
public enum CompressionType
{
    None,
    GZip,
    Deflate,
    Brotli
}