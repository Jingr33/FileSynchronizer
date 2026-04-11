using FileSynchronizer.Abstracts.Registries;
using FileSynchronizer.Abstracts.Synchronization;
using FileSynchronizer.DTOs;
using FileSynchronizer.Utilities;

namespace FileSynchronizer.Services.Synchronization;

public class SynchronizationManager(ILogger<SynchronizationManager> logger,
    IFileDataCacheRegistry fileDataCacheRegistry,
    IBackupUpdateManager backupUpdateManager)
    : ISynchronizationManager
{
    private ILogger<SynchronizationManager> Logger { get; } = logger;
    private IFileDataCacheRegistry FileDataCacheRegistry { get; } = fileDataCacheRegistry;
    private IBackupUpdateManager BackupUpdateManager { get; } = backupUpdateManager;

    public void Synchronize()
    {
        var sourceDir = PathHelper.GetSourceFolderPath();
        var destinationDir = PathHelper.GetReplicaFolderPath();
        var visitedPaths = new HashSet<string>();

        if (!FileDataCacheRegistry.GetAll().Any())
        {
            PrePopulateFileDataCache(destinationDir);
        }

        ExploreDirectory(sourceDir, visitedPaths);
        Logger.LogDebug("Explored directory: {Directory}", sourceDir);

        SearchForRenames(visitedPaths);

        BackupUpdateManager.UpdateBackup();

        ResetCacheState();
    }

    private void PrePopulateFileDataCache(string replicaDir)
    {
        foreach (var filePath in Directory.GetFiles(replicaDir, "*", SearchOption.AllDirectories))
        {
            FileDataCacheRegistry.AddOrUpdate(FileDataChaceRegistryHelper.GetInitialFileDataCache(filePath));
        }
    }

    private void ExploreDirectory(string sourceDir, HashSet<string> visitedPaths)
    {
        foreach (var file in Directory.GetFiles(sourceDir))
        {
            var fileInfo = new FileInfo(file);
            var filePath = new FilePath(file, DirectoryType.Source);

            visitedPaths.Add(filePath.RelativeNormalizedPath);

            if (!FileDataCacheRegistry.TryGet(filePath, out var fileDataCache))
            {
                FileDataCacheRegistry.AddOrUpdate(new FileDataCache
                {
                    Path = filePath,
                    Size = fileInfo.Length,
                    LastModified = fileInfo.LastWriteTimeUtc,
                    ChangeType = FileChangeType.New
                });

                continue;
            }

            if (fileDataCache!.HasSameMetadata(fileInfo))
            {
                fileDataCache.ChangeType = FileChangeType.Same;
            }
            else
            {
                FileDataCacheRegistry.AddOrUpdate(fileDataCache with
                {
                    Size = fileInfo.Length,
                    LastModified = fileInfo.LastWriteTimeUtc,
                    ChangeType = FileChangeType.Modified
                });
            }
        }

        foreach (var directory in Directory.GetDirectories(sourceDir))
        {
            ExploreDirectory(directory, visitedPaths);
        }
    }

    private void SearchForRenames(HashSet<string> visitedPaths)
    {
        var removedItems = FileDataCacheRegistry.GetAll()
            .Where(cached => !visitedPaths.Contains(cached.Path.RelativeNormalizedPath))
            .ToList();

        removedItems.ForEach(r => r.ChangeType = FileChangeType.Deleted);

        var newItems = FileDataCacheRegistry.GetAll().Where(x => x.ChangeType == FileChangeType.New).ToList();

        var renamedFilesCount = 0;
        foreach (var removedItem in removedItems)
        {
            var potentialRename = newItems.FirstOrDefault(n =>
                n.Size == removedItem.Size &&
                n.LastModified == removedItem.LastModified);

            if (potentialRename == null)
            {
                continue;
            }

            var renamedItem = new RenamedFileDataCache
            {
                Path = potentialRename.Path,
                Size = potentialRename.Size,
                LastModified = potentialRename.LastModified,
                ChangeType = FileChangeType.Renamed,
                MovedFrom = removedItem.Path
            };

            FileDataCacheRegistry.AddOrUpdate(renamedItem);
            FileDataCacheRegistry.Remove(removedItem.Path);

            newItems.Remove(potentialRename);
            renamedFilesCount++;
        }

        var fileWord = renamedFilesCount == 1 ? "filePath" : "files";
        Logger.LogDebug($"{renamedFilesCount} renamed {fileWord} detected and moved.");
    }

    private void ResetCacheState()
    {
        foreach (var cacheItem in FileDataCacheRegistry.GetAll())
        {
            cacheItem.ChangeType = FileChangeType.None;
        }

        Logger.LogDebug("Cache state reseted and ready for next synchronization.");
    }
}
