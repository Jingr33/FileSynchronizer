using FileSynchronizer.Abstracts.Backups.Synchronization;
using FileSynchronizer.Abstracts.Registries;
using FileSynchronizer.Abstracts.Synchronization;
using FileSynchronizer.DTOs;
using FileSynchronizer.Utilities;

namespace FileSynchronizer.Services.Backups.Synchronization;

public class MetadataSynchronizatonManager(
    ILogger<SynchronizationManagerBase> logger,
    IFileDataCacheRegistry fileDataCacheRegistry,
    IFilesUpdateManager filesUpdateManager)
    : SynchronizationManagerBase(logger, fileDataCacheRegistry, filesUpdateManager), IMetadataSynchronizationManager
{
    protected override void TryPrePopulateFileDataCache(string replicaDir)
    {
        if (!FileDataCacheRegistry.GetAll().Any())
        {
            foreach (var filePath in Directory.GetFiles(replicaDir, "*", SearchOption.AllDirectories))
            {
                FileDataCacheRegistry.AddOrUpdate(FileDataChaceHelper.CreateFileDataCache(filePath, DirectoryType.Replica));
            }
        }
    }

    protected override FileDataCache GetNewFileDataCache(string filePath)
        => FileDataChaceHelper.CreateFileDataCache(filePath, DirectoryType.Source, null, FileChangeType.New);

    protected override FileDataCache UpdateFileDataCache(FileDataCache baseFileDataCache, FileDataCache changedFileDataCache)
        => FileDataChaceHelper.ModifyFileDataCache(baseFileDataCache, changedFileDataCache.Size, changedFileDataCache.LastModified);

    protected override bool AreFilesIdentical(FileDataCache fileDataCache1, FileDataCache fileDataCache2)
    => fileDataCache1!.HasSameMetadata(fileDataCache2);
}
