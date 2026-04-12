using FileSynchronizer.Constants;

namespace FileSynchronizer.Utilities;

public static class PathHelper
{
    private readonly static string _baseDir = Directory.GetCurrentDirectory();

    public static string GetSourceFolderPath()
    {
        return Path.Combine(_baseDir, FolderNameConstants.SourceFolderName);
    }

    public static string GetReplicaFolderPath()
    {
        return Path.Combine(_baseDir, FolderNameConstants.ReplicaFolderName);
    }

    public static bool IsSourceFolderExists()
    {
        var sourceDir = GetSourceFolderPath();
        return Directory.Exists(sourceDir);
    }

    public static bool IsReplicaFolderExistsAndNotEmpty()
    {
        var replicaDir = GetReplicaFolderPath();
        return Directory.Exists(replicaDir) && Directory.EnumerateFileSystemEntries(replicaDir).Any();
    }

}
