using FileSynchronizer.Abstracts.Registries;
using FileSynchronizer.Abstracts.Synchronization;
using FileSynchronizer.Constants;
using FileSynchronizer.DTOs;
using FileSynchronizer.Utilities;

namespace FileSynchronizer.Services.Synchronization;

public class FilesUpdateManager(ILogger<FilesUpdateManager> logger, IFileDataCacheRegistry fileDataCacheRegistry)
    : IFilesUpdateManager
{
    private ILogger<FilesUpdateManager> Logger { get; } = logger;
    private IFileDataCacheRegistry FileDataCacheRegistry { get; } = fileDataCacheRegistry;

    public void UpdateBackup()
    {
        foreach (var fileDataCache in FileDataCacheRegistry.GetAll())
        {
            switch (fileDataCache.ChangeType)
            {
                case FileChangeType.New:
                case FileChangeType.Modified:
                    CopyBackupFileFromSource(fileDataCache);
                    break;
                case FileChangeType.Deleted:
                    DeleteBackupFile(fileDataCache);
                    break;
                case FileChangeType.Renamed:
                    MoveBackupFile((RenamedFileDataCache)fileDataCache);
                    break;
                default:
                    break;
            }
        }
    }

    private void CopyBackupFileFromSource(FileDataCache fileDataCache)
    {
        var sourcePath = fileDataCache.Path.GetSystemSpecificAbsolutePath(DirectoryType.Source);
        var destinationPath = fileDataCache.Path.GetSystemSpecificAbsolutePath(DirectoryType.Replica);

        var sourceLogPath = fileDataCache.Path.GetLoggablePath(DirectoryType.Source);

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);

            if (File.Exists(destinationPath))
            {
                FileAttributesHelper.RemoveReadOnlyAttribute(destinationPath);
            }

            File.Copy(sourcePath, destinationPath, true);

            if (fileDataCache.ChangeType == FileChangeType.New)
            {
                Logger.LogInformation($"New {sourceLogPath} file was detected and backed up in {FolderNameConstants.ReplicaFolderName} directory.");
            }
            else if (fileDataCache.ChangeType == FileChangeType.Modified)
            {
                Logger.LogInformation($"Modified file was detected. {sourceLogPath} file is now updated in {FolderNameConstants.ReplicaFolderName} directory.");
            }
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
        {
            Logger.LogWarning($"It is currently not possible to backup file {sourceLogPath}. File is probably used by different process or the application lacks necessary permissions to access it.");
        }
    }

    private void DeleteBackupFile(FileDataCache fileDataCache)
    {
        var destinationPath = fileDataCache.Path.GetSystemSpecificAbsolutePath(DirectoryType.Replica);
        var replicaLogPath = fileDataCache.Path.GetLoggablePath(DirectoryType.Replica);

        try
        {
            if (File.Exists(destinationPath))
            {
                FileAttributesHelper.RemoveReadOnlyAttribute(destinationPath);

                File.Delete(destinationPath);
                Logger.LogInformation($"Deleted {replicaLogPath} file because it was deleted from {FolderNameConstants.SourceFolderName}.");
            }
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
        {
            Logger.LogWarning($"It is currently not possible to delete file {replicaLogPath}. File is probably used by different process or the application lacks necessary permissions to access it.");
        }

        FileDataCacheRegistry.Remove(fileDataCache.Path);

        CleanUpEmptyDirectories(Path.GetDirectoryName(destinationPath));
    }

    private void MoveBackupFile(RenamedFileDataCache renamedFileDataCache)
    {
        var newDestinationPath = renamedFileDataCache.Path.GetSystemSpecificAbsolutePath(DirectoryType.Replica);
        var oldDestinationPath = renamedFileDataCache.MovedFrom!.GetSystemSpecificAbsolutePath(DirectoryType.Replica);

        var newReplicaLogPath = renamedFileDataCache.Path.GetLoggablePath(DirectoryType.Replica);
        var oldReplicaLogPath = renamedFileDataCache.MovedFrom!.GetLoggablePath(DirectoryType.Replica);

        try
        {
            if (File.Exists(oldDestinationPath))
            {
                bool wasReadOnly = FileAttributesHelper.RemoveReadOnlyAttribute(oldDestinationPath);

                Directory.CreateDirectory(Path.GetDirectoryName(newDestinationPath)!);
                File.Move(oldDestinationPath, newDestinationPath);

                if (wasReadOnly)
                {
                    FileAttributesHelper.AddReadOnlyAttribute(newDestinationPath);
                }

                Logger.LogInformation($"Renamed {oldReplicaLogPath} file to {newReplicaLogPath} because it was renamed in {FolderNameConstants.SourceFolderName}.");
            }
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
        {
            Logger.LogWarning($"It is currently not possible to move file {oldReplicaLogPath} to {newReplicaLogPath}. File is probably used by different process or the application lacks necessary permissions to access it.");
        }

        CleanUpEmptyDirectories(Path.GetDirectoryName(oldDestinationPath));
    }

    private void CleanUpEmptyDirectories(string? directoryPath)
    {
        var replicaRoot = PathHelper.GetReplicaFolderPath();
        replicaRoot = Path.TrimEndingDirectorySeparator(replicaRoot);

        while (!string.IsNullOrEmpty(directoryPath))
        {
            directoryPath = Path.TrimEndingDirectorySeparator(directoryPath);

            if (string.Equals(directoryPath, replicaRoot, StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            if (!Directory.Exists(directoryPath))
            {
                break;
            }

            if (Directory.EnumerateFileSystemEntries(directoryPath).Any())
            {
                break;
            }

            try
            {
                Directory.Delete(directoryPath);
                Logger.LogInformation($"Deleted empty {directoryPath} directory from {FolderNameConstants.ReplicaFolderName} folder.");
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                Logger.LogDebug($"It is currently not possible to remove {directoryPath} in {FolderNameConstants.ReplicaFolderName}. File is probably used by different process or the application lacks necessary permissions to access it.");
                return;
            }

            directoryPath = Path.GetDirectoryName(directoryPath);
        }
    }
}
