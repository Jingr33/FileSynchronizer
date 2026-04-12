using FileSynchronizer.Abstracts.Backups.Synchronization;

namespace FileSynchronizer.Abstracts.Handlers;

public interface ISynchronizationJobHandler
{
    void ExecuteSynchronizationJob();
    void ExecuteDeepSynchronizationJob();
    void ExecuteSpecificSynchronization<TSynchronizationManager>(TSynchronizationManager synchronizationManager)
        where TSynchronizationManager : ISynchronizationManager;
}
