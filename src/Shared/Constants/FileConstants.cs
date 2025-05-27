namespace FileStoreService.Shared.Constants;

public static class FileConstants
{
    // File size limits (configurable via environment variables)
    public const long SINGLE_FILE_SIZE_LIMIT = 100_000_000; // 100MB - can be overridden by env var
    public const long BULK_UPLOAD_SIZE_LIMIT = 500_000_000; // 500MB - can be overridden by env var
    public const long DOWNLOAD_CHUNK_SIZE = 8192; // 8KB chunks for streaming

    // Rate limiting policy names
    public const string RATE_LIMIT_UPLOAD_POLICY = "FileUploadPolicy";
    public const string RATE_LIMIT_BULK_UPLOAD_POLICY = "BulkUploadPolicy";
    public const string RATE_LIMIT_DOWNLOAD_POLICY = "FileDownloadPolicy";
    public const string RATE_LIMIT_SEARCH_POLICY = "FileSearchPolicy";

    // Cache keys
    public const string CACHE_KEY_SEARCH_SUGGESTIONS = "search_suggestions";
    public const string CACHE_KEY_FILE_STATS = "file_stats_{0}";
    public const string CACHE_KEY_USER_UPLOADS = "user_uploads_{0}";

    // HTTP Headers
    public const string HEADER_CORRELATION_ID = "X-Correlation-ID";
    public const string HEADER_REQUEST_ID = "X-Request-ID";
    public const string HEADER_API_VERSION = "X-API-Version";
    public const string HEADER_RATE_LIMIT_REMAINING = "X-RateLimit-Remaining";

    // Content types
    public const string CONTENT_TYPE_ZIP = "application/zip";
    public const string CONTENT_TYPE_OCTET_STREAM = "application/octet-stream";

    // Default values
    public const int DEFAULT_SEARCH_PAGE_SIZE = 20;
    public const int MAX_SEARCH_PAGE_SIZE = 100;
    public const int DEFAULT_BULK_DOWNLOAD_LIMIT = 50;

    // Environment variable names
    public const string ENV_MAX_FILE_SIZE = "FILE_MAX_SINGLE_SIZE";
    public const string ENV_MAX_BULK_SIZE = "FILE_MAX_BULK_SIZE";
    public const string ENV_STORAGE_PATH = "FILE_STORAGE_PATH";
    public const string ENV_TEMP_PATH = "FILE_TEMP_PATH";
    public const string ENV_CDN_BASE_URL = "CDN_BASE_URL";
}