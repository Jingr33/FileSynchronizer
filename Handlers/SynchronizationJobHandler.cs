using FileSynchronizer.Abstracts.Handlers;
using FileSynchronizer.Abstracts.Synchronization;
using FileSynchronizer.Utilities;
using Hangfire;

namespace FileSynchronizer.Handlers;

public class SynchronizationJobHandler(
    ILogger<SynchronizationJobHandler> logger,
    ISynchronizationManager synchronizationManager,
    IFullBackupCreationManager backupCreationManager)
    : ISynchronizationJobHandler
{
    private ILogger<SynchronizationJobHandler> Logger { get; } = logger;
    private ISynchronizationManager SynchronizationManager { get; } = synchronizationManager;
    private IFullBackupCreationManager BackupCreationManager { get; } = backupCreationManager;

    [DisableConcurrentExecution(timeoutInSeconds: 60)]
    public void ExecuteSynchronizationJob()
    {
        if (PathHelper.IsReplicaFolderExistsAndNotEmpty())
        {
            Logger.LogDebug("Starting synchronization...");
            SynchronizationManager.Synchronize();
        }
        else
        {
            Logger.LogDebug("Replica folder does not exist or is empty. Creating full backup.");
            BackupCreationManager.CreateBackup();
        }

        Logger.LogInformation("Synchronization Completed.");
    }
}
