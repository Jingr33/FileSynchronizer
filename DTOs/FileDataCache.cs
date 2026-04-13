namespace FileSynchronizer.DTOs;

public record FileDataCache
{
    public FilePath Path { get; init; } = default!;
    public long Size { get; init; }
    public DateTime LastModified { get; init; }
    public long? Hash { get; init; }

    public FileChangeType ChangeType { get; set; } = FileChangeType.Same;

    public bool HasSameMetadata(FileDataCache otherCacheItem)
        => Size == otherCacheItem.Size && LastModified == otherCacheItem.LastModified;

    public bool HasSameHash(FileDataCache otherCacheItem)
        => Hash == otherCacheItem.Hash;
}
