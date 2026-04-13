using FileSynchronizer.Abstracts.Registries;
using FileSynchronizer.DTOs;

namespace FileSynchronizer.Services.Registries;

public class FileDataCacheRegistry : IFileDataCacheRegistry
{
    private readonly Dictionary<FilePath, FileDataCache> _cache = [];

    public void AddOrUpdate(FileDataCache fileDataCache)
    {
        _cache[fileDataCache.Path] = fileDataCache;
    }

    public bool TryGet(FilePath filePath, out FileDataCache? fileDataCache)
    {
        return _cache.TryGetValue(filePath, out fileDataCache);
    }

    public void Remove(FilePath filePath)
    {
        _cache.Remove(filePath);
    }

    public IEnumerable<FileDataCache> GetAll()
    {
        return _cache.Values;
    }
}
