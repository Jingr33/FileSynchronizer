using FileSynchronizer.Abstracts.Backups.Synchronization;
using FileSynchronizer.Abstracts.Handlers;
using FileSynchronizer.Abstracts.Synchronization;
using FileSynchronizer.Configuration;
using FileSynchronizer.Utilities;
using Hangfire;

namespace FileSynchronizer.Services.Handlers;

public class SynchronizationJobHandler(
    ILogger<SynchronizationJobHandler> logger,
    IMetadataSynchronizationManager synchronizationManager,
    IHashSynchronizationManager hashSynchronizationManager,
    IFullBackupCreationManager backupCreationManager,
    ApplicationOptions applicationOptions,
    IBackgroundJobClient backgroundJobClient)
    : ISynchronizationJobHandler
{
    private ILogger<SynchronizationJobHandler> Logger { get; } = logger;
    private IMetadataSynchronizationManager MetadataSynchronizationManager { get; } = synchronizationManager;
    private IHashSynchronizationManager HashSynchronizationManager { get; } = hashSynchronizationManager;
    private IFullBackupCreationManager BackupCreationManager { get; } = backupCreationManager;
    private ApplicationOptions ApplicationOptions { get; } = applicationOptions;
    private IBackgroundJobClient BackgroundJobClient { get; } = backgroundJobClient;

    [DisableConcurrentExecution(timeoutInSeconds: 60)]
    public void ExecuteSynchronizationJob()
    {
        Logger.LogInformation("Time for planned metadata based synchronization");
        ExecuteSpecificSynchronization(MetadataSynchronizationManager);
        Logger.LogInformation("Metadata based synchronization completed.");

        var delay = IntervalParser.ParseToTimeSpan(ApplicationOptions.BackupInterval);
        BackgroundJobClient.Schedule<ISynchronizationJobHandler>(h => h.ExecuteSynchronizationJob(), delay);
        Logger.LogInformation($"Next metadata synchronization wil be executed in {IntervalParser.GetReadableInterval(ApplicationOptions.BackupInterval)}.");
    }

    [DisableConcurrentExecution(timeoutInSeconds: 60)]
    public void ExecuteDeepSynchronizationJob()
    {
        Logger.LogInformation("Time for planned deep hash based synchronization");
        ExecuteSpecificSynchronization(HashSynchronizationManager);
        Logger.LogInformation("Deep hash based synchronization completed.");

        var delay = IntervalParser.ParseToTimeSpan(ApplicationOptions.DeepBackupInterval!);
        BackgroundJobClient.Schedule<ISynchronizationJobHandler>(h => h.ExecuteDeepSynchronizationJob(), delay);
        Logger.LogInformation($"Next deep hash based synchronization will be executed in {IntervalParser.GetReadableInterval(ApplicationOptions.DeepBackupInterval!)}.");
    }

    public void ExecuteSpecificSynchronization<TSynchronizationManager>(TSynchronizationManager synchronizationManager)
        where TSynchronizationManager : ISynchronizationManager
    {
        if (PathHelper.IsReplicaFolderExistsAndNotEmpty())
        {
            Logger.LogInformation("Starting synchronization...");
            synchronizationManager.Synchronize();
        }
        else
        {
            Logger.LogInformation("Replica folder does not exist or is empty. Creating full backup.");
            BackupCreationManager.CreateBackup();
        }
    }
}
