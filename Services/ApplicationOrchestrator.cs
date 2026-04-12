using FileSynchronizer.Abstracts;
using FileSynchronizer.Abstracts.Backups.Synchronization;
using FileSynchronizer.Abstracts.Handlers;
using FileSynchronizer.Abstracts.Jobs;
using FileSynchronizer.Configuration;
using FileSynchronizer.Utilities;

namespace FileSynchronizer.Services;

public class ApplicationOrchestrator(
    ILogger<ApplicationOrchestrator> logger,
    ISynchronizationJobManager jobManager,
    ISynchronizationJobHandler synchronizationJobHandler,
    IMetadataSynchronizationManager metadataSynchronizationManager,
    IHashSynchronizationManager hashSynchronizationManager,
    ApplicationOptions applicationOptions)
    : IApplicationOrchestrator
{
    private ILogger<ApplicationOrchestrator> Logger { get; } = logger;
    private ISynchronizationJobManager JobManager { get; } = jobManager;
    private ISynchronizationJobHandler SynchronizationJobHandler { get; } = synchronizationJobHandler;
    private IMetadataSynchronizationManager MetadataSynchronizationManager { get; } = metadataSynchronizationManager;
    private IHashSynchronizationManager HashSynchronizationManager { get; } = hashSynchronizationManager;
    private ApplicationOptions ApplicationOptions { get; } = applicationOptions;

    public void Start()
    {
        if (!PathHelper.IsSourceFolderExists())
        {
            throw new DirectoryNotFoundException($"Source folder was not found. The system cannot start synchronization.");
        }

        Logger.LogDebug("Starting the application...");

        JobManager.ScheduleInitialJobs();

        if (!string.IsNullOrWhiteSpace(ApplicationOptions.BackupInterval))
        {
            Logger.LogInformation("Initial metadata based synchronization will be executed.");
            SynchronizationJobHandler.ExecuteSpecificSynchronization(MetadataSynchronizationManager);
            Logger.LogInformation("Initial metadata based synchronization has been completed.");
        }
        else if (!string.IsNullOrWhiteSpace(ApplicationOptions.DeepBackupInterval))
        {
            Logger.LogInformation("Initial hash based synchronization will be executed.");
            SynchronizationJobHandler.ExecuteSpecificSynchronization(HashSynchronizationManager);
            Logger.LogInformation("Initial hash based synchronization has been completed.");
        }
        else
        {
            Logger.LogInformation("No recurring intervals provided. Application will now exit.");
            Environment.Exit(0);
        }
    }
}
