using FileSynchronizer.Abstracts.Handlers;
using FileSynchronizer.Abstracts.Jobs;
using FileSynchronizer.Configuration;
using FileSynchronizer.Utilities;
using Hangfire;

namespace FileSynchronizer.Services.Jobs;

public class SynchronizationJobManager(ILogger<SynchronizationJobManager> logger, ApplicationOptions applicationOptions) : ISynchronizationJobManager
{
    private ApplicationOptions ApplicationOptions { get; } = applicationOptions;
    private ILogger<SynchronizationJobManager> Logger { get; } = logger;

    public void ScheduleInitialJobs()
    {
        if (!string.IsNullOrWhiteSpace(ApplicationOptions.BackupInterval))
        {
            var nextMetadataSyncExecution = IntervalParser.ParseToTimeSpan(ApplicationOptions.BackupInterval);
            BackgroundJob.Schedule<ISynchronizationJobHandler>(handler => handler.ExecuteSynchronizationJob(), nextMetadataSyncExecution);
            Logger.LogInformation($"Metadata synchronization will be executed every {IntervalParser.GetReadableInterval(ApplicationOptions.BackupInterval)}.");
        }

        if (!string.IsNullOrWhiteSpace(ApplicationOptions.DeepBackupInterval))
        {
            var nextHashSyncExecution = IntervalParser.ParseToTimeSpan(ApplicationOptions.DeepBackupInterval);
            BackgroundJob.Schedule<ISynchronizationJobHandler>(handler => handler.ExecuteDeepSynchronizationJob(), nextHashSyncExecution);
            Logger.LogInformation($"Deep hash synchronization will be executed every {IntervalParser.GetReadableInterval(ApplicationOptions.DeepBackupInterval)}.");
        }
    }
}
