using FileSynchronizer.Abstracts.Backups.Synchronization;
using FileSynchronizer.Abstracts.Registries;
using FileSynchronizer.Abstracts.Synchronization;
using FileSynchronizer.DTOs;
using FileSynchronizer.Utilities;
using System.IO.Hashing;

namespace FileSynchronizer.Services.Backups.Synchronization;

public class HashSynchronizationManager(
    ILogger<SynchronizationManagerBase> logger,
    IFileDataCacheRegistry fileDataCacheRegistry,
    IFilesUpdateManager filesUpdateManager)
    : SynchronizationManagerBase(logger, fileDataCacheRegistry, filesUpdateManager), IHashSynchronizationManager
{
    protected override void TryPrePopulateFileDataCache(string replicaDir)
    {
        var allFileDataCaches = FileDataCacheRegistry.GetAll();

        if (!allFileDataCaches.Any())
        {
            foreach (var filePath in Directory.GetFiles(replicaDir, "*", SearchOption.AllDirectories))
            {
                var fileHash = CalculateFileHash(filePath);
                FileDataCacheRegistry.AddOrUpdate(FileDataChaceHelper.CreateFileDataCache(filePath, DirectoryType.Replica, fileHash));
            }
        }
        else
        {
            foreach (var fileDataCache in allFileDataCaches)
            {
                if (fileDataCache.Hash is null)
                {
                    var sourcePath = fileDataCache.Path.GetSystemSpecificAbsolutePath(DirectoryType.Source);

                    if (!File.Exists(sourcePath))
                    {
                        continue;
                    }

                    var fileHash = CalculateFileHash(sourcePath);
                    FileDataCacheRegistry.AddOrUpdate(FileDataChaceHelper.ModifyFileDataCache(fileDataCache, hash: fileHash));
                }
            }
        }
    }

    protected override FileDataCache GetNewFileDataCache(string filePath)
    {
        var fileHash = CalculateFileHash(filePath);
        return FileDataChaceHelper.CreateFileDataCache(filePath, DirectoryType.Source, fileHash, FileChangeType.New);
    }

    protected override FileDataCache UpdateFileDataCache(FileDataCache baseFileDataCache, FileDataCache changedFileDataCache)
    {
        var fileHash = CalculateFileHash(baseFileDataCache.Path.GetSystemSpecificAbsolutePath(DirectoryType.Source));
        return FileDataChaceHelper.ModifyFileDataCache(baseFileDataCache, changedFileDataCache.Size, changedFileDataCache.LastModified, fileHash);
    }

    protected override bool AreFilesIdentical(FileDataCache fileDataCache1, FileDataCache fileDataCache2)
    => fileDataCache1.Hash == fileDataCache2.Hash;

    private static long CalculateFileHash(string filePath)
    {
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

        var hashAlgorithm = new XxHash64();
        hashAlgorithm.Append(stream);
        var hashBytes = hashAlgorithm.GetCurrentHash();
        return BitConverter.ToInt64(hashBytes);
    }
}
