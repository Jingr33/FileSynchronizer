using CommandLine;

namespace FileSynchronizer.Configuration;

public class ApplicationOptions
{
    [Option('l', "logFile", Required = true, HelpText = "Path to the log file")]
    public string LogFile { get; init; } = string.Empty;

    [Option('b', "backupPeriod", Default = 5, HelpText = "Backup period interval in minutes")]
    public int BackupPeriod { get; init; }
}
