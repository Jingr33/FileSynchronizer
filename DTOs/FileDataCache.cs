namespace FileSynchronizer.DTOs;

public record FileDataCache
{
    public FilePath Path { get; init; } = default!;
    public long Size { get; init; }
    public DateTime LastModified { get; init; }
    public long? Hash { get; init; }

    public FileChangeType ChangeType { get; set; } = FileChangeType.Same;

    public bool HasSameMetadata(FileInfo otherFileInfo)
    {
        return Size == otherFileInfo.Length && LastModified == otherFileInfo.LastWriteTimeUtc;
    }

    public bool HasSameMetadata(FileDataCache otherCacheItem)
    {
        return Size == otherCacheItem.Size && LastModified == otherCacheItem.LastModified;
    }
}
