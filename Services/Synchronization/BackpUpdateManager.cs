using FileSynchronizer.Abstracts.Registries;
using FileSynchronizer.Abstracts.Synchronization;
using FileSynchronizer.Constants;
using FileSynchronizer.DTOs;
using FileSynchronizer.Utilities;

namespace FileSynchronizer.Services.Synchronization;

public class BackpUpdateManager(ILogger<BackpUpdateManager> logger, IFileDataCacheRegistry fileDataCacheRegistry)
    : IBackupUpdateManager
{
    private ILogger<BackpUpdateManager> Logger { get; } = logger;
    private IFileDataCacheRegistry FileDataCacheRegistry { get; } = fileDataCacheRegistry;

    public void UpdateBackup()
    {
        foreach (var fileDataCache in FileDataCacheRegistry.GetAll())
        {
            switch (fileDataCache.ChangeType)
            {
                case FileChangeType.New:
                case FileChangeType.Modified:
                    CreateBackupFile(fileDataCache);
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

    private void CreateBackupFile(FileDataCache fileDataCache)
    {
        var sourcePath = fileDataCache.Path.GetSystemSpecificAbsolutePath(DirectoryType.Source);
        var destinationPath = fileDataCache.Path.GetSystemSpecificAbsolutePath(DirectoryType.Replica);

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
            File.Copy(sourcePath, destinationPath, true);
            Logger.LogInformation($"New {sourcePath} file was detected and backed up in {FolderNameConstants.ReplicaFolderName}.");
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
        {
            Logger.LogWarning($"It is currently not possible to backup file {sourcePath}. File is probably used by different process or the application lacks necessary permissions to access it.");
        }
    }

    private void DeleteBackupFile(FileDataCache fileDataCache)
    {
        var destinationPath = fileDataCache.Path.GetSystemSpecificAbsolutePath(DirectoryType.Replica);

        try
        {
            if (File.Exists(destinationPath))
            {
                File.Delete(destinationPath);
                Logger.LogInformation($"Deleted {destinationPath} file from {FolderNameConstants.ReplicaFolderName} folder because it was deleted from {FolderNameConstants.SourceFolderName}.");
            }
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
        {
            Logger.LogWarning($"It is currently not possible to delete file {destinationPath} from {FolderNameConstants.ReplicaFolderName} folder. File is probably used by different process or the application lacks necessary permissions to access it.");
        }

        FileDataCacheRegistry.Remove(fileDataCache.Path);

        CleanUpEmptyDirectories(Path.GetDirectoryName(destinationPath));
    }

    private void MoveBackupFile(RenamedFileDataCache renamedFileDataCache)
    {
        var newDestinationPath = renamedFileDataCache.Path.GetSystemSpecificAbsolutePath(DirectoryType.Replica);
        var oldDestinationPath = renamedFileDataCache.MovedFrom!.GetSystemSpecificAbsolutePath(DirectoryType.Replica);

        try
        {
            if (File.Exists(oldDestinationPath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(newDestinationPath)!);
                File.Move(oldDestinationPath, newDestinationPath);
                Logger.LogInformation($"Renamed {oldDestinationPath} file to {newDestinationPath} in {FolderNameConstants.ReplicaFolderName} folder because it was renamed in {FolderNameConstants.SourceFolderName}.");
            }
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
        {
            Logger.LogWarning($"It is currently not possible to move file {oldDestinationPath} to {newDestinationPath} in {FolderNameConstants.ReplicaFolderName} folder. File is probably used by different process or the application lacks necessary permissions to access it.");
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
