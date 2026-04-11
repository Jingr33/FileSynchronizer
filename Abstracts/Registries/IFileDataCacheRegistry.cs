using FileSynchronizer.DTOs;

namespace FileSynchronizer.Abstracts.Registries;

public interface IFileDataCacheRegistry
{
    void AddOrUpdate(FileDataCache fileDataCache);
    bool TryGet(FilePath filePath, out FileDataCache? fileDataCache);
    void Remove(FilePath filePath);
    IEnumerable<FileDataCache> GetAll();
}
