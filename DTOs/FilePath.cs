using FileSynchronizer.Utilities;

namespace FileSynchronizer.DTOs;

public record FilePath
{
    public string RelativeNormalizedPath { get; init; }

    public FilePath(string absoluteFilePath, DirectoryType directoryType)
    {
        string baseDirectory = GetBaseDirectory(directoryType);

        string relativePath = absoluteFilePath.StartsWith(baseDirectory, StringComparison.OrdinalIgnoreCase)
            ? absoluteFilePath.Substring(baseDirectory.Length)
            : absoluteFilePath;

        relativePath = relativePath.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        RelativeNormalizedPath = relativePath.Replace('\\', '/');
    }

    public string GetSystemSpecificAbsolutePath(DirectoryType directoryType)
    {
        var baseDir = GetBaseDirectory(directoryType);
        var systemSpecificPath = RelativeNormalizedPath.Replace('/', Path.DirectorySeparatorChar);

        return Path.Combine(baseDir, systemSpecificPath);
    }

    public static string GetBaseDirectory(DirectoryType directoryType)
        => directoryType switch
        {
            DirectoryType.Source => PathHelper.GetSourceFolderPath(),
            DirectoryType.Replica => PathHelper.GetReplicaFolderPath(),
            _ => throw new ArgumentException($"Unsupported directory type: {directoryType}")
        };
}
