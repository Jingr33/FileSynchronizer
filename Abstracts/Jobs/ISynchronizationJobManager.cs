namespace FileSynchronizer.Abstracts.Jobs;

public interface ISynchronizationJobManager
{
    void CreateRecurringJob();
    void TriggerJob();
}
