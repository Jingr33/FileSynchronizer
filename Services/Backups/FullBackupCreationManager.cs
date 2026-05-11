using FileSynchronizer.Abstracts.Registries;
using FileSynchronizer.Abstracts.Synchronization;
using FileSynchronizer.DTOs;
using FileSynchronizer.Utilities;

namespace FileSynchronizer.Services.Synchronization;

public class FullBackupCreationManager(
    ILogger<FullBackupCreationManager> logger,
    IFileDataCacheRegistry fileDataCacheRegistry)
    : IFullBackupCreationManager
{
    private ILogger<FullBackupCreationManager> Logger { get; } = logger;
    private IFileDataCacheRegistry FileDataCacheRegistry { get; } = fileDataCacheRegistry;

    public void CreateBackup()
    {
        var sourceDir = PathHelper.GetSourceFolderPath();
        var destinationDir = PathHelper.GetReplicaFolderPath();

        CopyDirectory(sourceDir, destinationDir);
        Logger.LogDebug("Complete backup creation process finished successfully.");
    }

    private void CopyDirectory(string sourceDir, string destinationDir)
    {
        Directory.CreateDirectory(destinationDir);

        foreach (var file in Directory.GetFiles(sourceDir))
        {
            var destFile = Path.Combine(destinationDir, Path.GetFileName(file));

            if (File.Exists(destFile))
            {
                FileAttributesHelper.RemoveReadOnlyAttribute(file);
            }

            try
            {
                File.Copy(file, destFile, overwrite: true);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                Logger.LogWarning($"It is currently not possible to move a file {file}. File is probably used by different process or the application lacks necessary permissions to access it.");
            }


            FileDataCacheRegistry.AddOrUpdate(FileDataChaceHelper.CreateFileDataCache(destFile, DirectoryType.Replica));
        }

        foreach (var directory in Directory.GetDirectories(sourceDir))
        {
            var destDir = Path.Combine(destinationDir, Path.GetFileName(directory));
            CopyDirectory(directory, destDir);
        }
    }
}
