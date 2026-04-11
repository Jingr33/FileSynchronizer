namespace FileSynchronizer.DTOs;

public record FileDataCache
{
    public FilePath Path { get; init; } = default!;
    public long? Size { get; init; }
    public DateTime? LastModified { get; init; }
    public int? Hash { get; init; }

    public FileChangeType ChangeType { get; set; } = FileChangeType.Same;

    public bool HasSameMetadata(FileInfo otherFileInfo)
    {
        return Size == otherFileInfo.Length && LastModified == otherFileInfo.LastWriteTimeUtc;
    }
}
