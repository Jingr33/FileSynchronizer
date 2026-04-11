using FileSynchronizer.DTOs;

namespace FileSynchronizer.Utilities;

public static class FileDataChaceRegistryHelper
{
    public static FileDataCache GetInitialFileDataCache(string filePath)
    {
        var fileInfo = new FileInfo(filePath);

        return new FileDataCache
        {
            Path = new FilePath(filePath, DirectoryType.Replica),
            LastModified = fileInfo.LastWriteTimeUtc,
            Size = fileInfo.Length
        };
    }
}
