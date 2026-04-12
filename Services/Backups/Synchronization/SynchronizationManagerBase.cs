using FileSynchronizer.Abstracts.Backups.Synchronization;
using FileSynchronizer.Abstracts.Registries;
using FileSynchronizer.Abstracts.Synchronization;
using FileSynchronizer.DTOs;
using FileSynchronizer.Utilities;

namespace FileSynchronizer.Services.Backups.Synchronization;

public abstract class SynchronizationManagerBase(
    ILogger<SynchronizationManagerBase> logger,
    IFileDataCacheRegistry fileDataCacheRegistry,
    IFilesUpdateManager filesUpdateManager)
    : ISynchronizationManager
{
    protected ILogger<SynchronizationManagerBase> Logger { get; } = logger;
    protected IFileDataCacheRegistry FileDataCacheRegistry { get; } = fileDataCacheRegistry;
    protected IFilesUpdateManager FilesUpdateManager { get; } = filesUpdateManager;

    private SynchronizationSummary _synchronizationsummary = new();

    public void Synchronize()
    {
        var sourceDir = PathHelper.GetSourceFolderPath();
        var destinationDir = PathHelper.GetReplicaFolderPath();
        var visitedPaths = new HashSet<string>();

        TryPrePopulateFileDataCache(destinationDir);

        ExploreSourceDirectory(sourceDir, visitedPaths);
        Logger.LogDebug("Explored directory: {Directory}", sourceDir);

        SearchForRemovesAndRenames(visitedPaths);

        FilesUpdateManager.UpdateBackup();

        Logger.LogInformation(_synchronizationsummary.GetSummaryText());
        ResetCacheState();
    }

    protected abstract void TryPrePopulateFileDataCache(string replicaDir);

    private void ExploreSourceDirectory(string sourceDir, HashSet<string> visitedPaths)
    {
        foreach (var stringFilePath in Directory.GetFiles(sourceDir))
        {
            var filePath = new FilePath(stringFilePath, DirectoryType.Source);

            visitedPaths.Add(filePath.RelativeNormalizedPath);
            var sourceFileDataCache = GetNewFileDataCache(stringFilePath);

            if (!FileDataCacheRegistry.TryGet(filePath, out var backupFileDataCache))
            {
                // NEW file
                FileDataCacheRegistry.AddOrUpdate(sourceFileDataCache);
                _synchronizationsummary.IncrementNewFiles();
                continue;
            }

            if (AreFilesIdentical(backupFileDataCache!, sourceFileDataCache))
            {
                // IDENTICAL file
                backupFileDataCache!.ChangeType = FileChangeType.Same;
            }
            else
            {
                // MODIFIED file
                FileDataCacheRegistry.AddOrUpdate(UpdateFileDataCache(backupFileDataCache!, sourceFileDataCache));
                _synchronizationsummary.IncrementModifiedFiles();
            }
        }

        foreach (var directory in Directory.GetDirectories(sourceDir))
        {
            ExploreSourceDirectory(directory, visitedPaths);
        }
    }

    protected abstract FileDataCache GetNewFileDataCache(string filePath);

    protected abstract FileDataCache UpdateFileDataCache(FileDataCache baseFileDataCache, FileDataCache changedFileDataCache);

    protected abstract bool AreFilesIdentical(FileDataCache fileDataCache1, FileDataCache fileDataCache2);

    private void SearchForRemovesAndRenames(HashSet<string> visitedPaths)
    {
        var removedItems = FileDataCacheRegistry.GetAll()
            .Where(cached => !visitedPaths.Contains(cached.Path.RelativeNormalizedPath))
            .ToList();

        foreach (var removedItem in removedItems)
        {
            removedItem.ChangeType = FileChangeType.Deleted;
            _synchronizationsummary.IncrementDeletedFiles();
        }

        var newItems = FileDataCacheRegistry.GetAll().Where(x => x.ChangeType == FileChangeType.New).ToList();

        foreach (var removedItem in removedItems)
        {
            var potentialRename = newItems.FirstOrDefault(n => AreFilesIdentical(n, removedItem));

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
            _synchronizationsummary.ApplyRenamedFilesDetection();
        }
    }

    private void ResetCacheState()
    {
        foreach (var cacheItem in FileDataCacheRegistry.GetAll())
        {
            cacheItem.ChangeType = FileChangeType.None;
        }

        _synchronizationsummary = new SynchronizationSummary();
        Logger.LogDebug("Cache state reseted and ready for next synchronization.");
    }
}
