namespace FileSynchronizer.DTOs;

public record SynchronizationSummary
{
    public int NewFiles { get; private set; } = 0;
    public int ModifiedFiles { get; private set; } = 0;
    public int DeletedFiles { get; private set; } = 0;
    public int RenamedFiles { get; private set; } = 0;
    public int TotalFilesProcessed => NewFiles + ModifiedFiles + DeletedFiles + RenamedFiles;

    public void IncrementNewFiles() => NewFiles++;

    public void IncrementModifiedFiles() => ModifiedFiles++;

    public void IncrementDeletedFiles() => DeletedFiles++;

    public void IncrementRenamedFiles() => RenamedFiles++;

    public string GetSummaryText()
        => $"Synchronization Summary: {TotalFilesProcessed} files processed - {NewFiles} new, {ModifiedFiles} modified, {DeletedFiles} deleted, {RenamedFiles} renamed.";
}
