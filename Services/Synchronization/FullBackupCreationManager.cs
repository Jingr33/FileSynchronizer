using FileSynchronizer.Abstracts.Registries;
using FileSynchronizer.Abstracts.Synchronization;
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

        Directory.CreateDirectory(destinationDir);

        CopyDirectory(sourceDir, destinationDir);
        Logger.LogDebug("Complete backup creation process finished successfully.");
    }

    private void CopyDirectory(string sourceDir, string destinationDir)
    {
        foreach (var file in Directory.GetFiles(sourceDir))
        {
            var destFile = Path.Combine(destinationDir, Path.GetFileName(file));
            File.Copy(file, destFile, overwrite: true);

            FileDataCacheRegistry.AddOrUpdate(FileDataChaceRegistryHelper.GetInitialFileDataCache(destFile));
        }

        foreach (var directory in Directory.GetDirectories(sourceDir))
        {
            var destDir = Path.Combine(destinationDir, Path.GetFileName(directory));
            CopyDirectory(directory, destDir);
        }
    }
}
