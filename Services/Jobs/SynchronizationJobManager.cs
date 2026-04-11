using FileSynchronizer.Abstracts.Handlers;
using FileSynchronizer.Abstracts.Jobs;
using FileSynchronizer.Configuration;
using FileSynchronizer.Constants;
using Hangfire;

namespace FileSynchronizer.Services.Jobs;

public class SynchronizationJobManager(ILogger<SynchronizationJobManager> logger, ApplicationOptions applicationOptions) : ISynchronizationJobManager
{
    private ApplicationOptions ApplicationOptions { get; } = applicationOptions;
    private ILogger<SynchronizationJobManager> Logger { get; } = logger;

    public void CreateRecurringJob()
    {
        RecurringJob.AddOrUpdate<ISynchronizationJobHandler>(
            HangfireJobIdConstants.SynchronizationJobId,
            handler => handler.ExecuteSynchronizationJob(),
            $"*/{ApplicationOptions.BackupPeriod} * * * *"
        );

        Logger.LogDebug($"Created recurring job with id {HangfireJobIdConstants.SynchronizationJobId}");
    }

    public void TriggerJob()
    {
        RecurringJob.TriggerJob(HangfireJobIdConstants.SynchronizationJobId);
        Logger.LogDebug($"Triggered job with id {HangfireJobIdConstants.SynchronizationJobId}");
    }
}
