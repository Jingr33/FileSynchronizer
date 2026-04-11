namespace FileSynchronizer.DTOs;

public record RenamedFileDataCache : FileDataCache
{
    public FilePath MovedFrom { get; init; } = default!;
}
