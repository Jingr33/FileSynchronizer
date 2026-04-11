using FileSynchronizer.Abstracts;
using FileSynchronizer.Abstracts.Jobs;
using FileSynchronizer.Utilities;

namespace FileSynchronizer.Services;

public class ApplicationOrchestrator(ILogger<ApplicationOrchestrator> logger, ISynchronizationJobManager jobManager) : IApplicationOrchestrator
{
    private ILogger<ApplicationOrchestrator> Logger { get; } = logger;
    private ISynchronizationJobManager JobManager { get; } = jobManager;

    public void Start()
    {
        if (!PathHelper.IsSourceFolderExists())
        {
            throw new DirectoryNotFoundException($"Source folder was not found. The system cannot start synchronization.");
        }

        Logger.LogDebug("Starting the application...");

        JobManager.CreateRecurringJob();
        JobManager.TriggerJob();
    }
}
