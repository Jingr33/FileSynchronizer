using FileSynchronizer.DTOs;

namespace FileSynchronizer.Utilities;

public static class FileDataChaceHelper
{
    public static FileDataCache CreateFileDataCache(string filePath, DirectoryType directoryType, long? hash = null, FileChangeType changeType = FileChangeType.None)
    {
        var fileInfo = new FileInfo(filePath);

        return new FileDataCache
        {
            Path = new FilePath(filePath, directoryType),
            LastModified = fileInfo.LastWriteTimeUtc,
            Size = fileInfo.Length,
            Hash = hash,
            ChangeType = changeType,
        };
    }

    public static FileDataCache ModifyFileDataCache(
        FileDataCache baseFileDataCache,
        long? size = null,
        DateTime? LastModified = null,
        long? hash = null)
        => new()
        {
            Path = baseFileDataCache.Path,
            Size = size ?? baseFileDataCache.Size,
            LastModified = LastModified ?? baseFileDataCache.LastModified,
            Hash = hash ?? baseFileDataCache.Hash,
            ChangeType = FileChangeType.Modified,
        };
}
