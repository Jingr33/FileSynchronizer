using CommandLine;
using FileSynchronizer.Validators;

namespace FileSynchronizer.Configuration;

public class ApplicationOptions
{
    [Option('l', "logFile", Required = true, HelpText = "Path to the log file")]
    [ValidFilePath]
    public string LogFile { get; init; } = string.Empty;

    [Option('b', "backupInterval", Required = false, HelpText = "Backup interval (e.g., '15m', '2h', '1d12h').")]
    [ValidInterval]
    public string BackupInterval { get; init; } = string.Empty;

    [Option('d', "deepBackupInterval", Required = false, HelpText = "Deep backup interval (e.g., '15m', '2h', '1d12h'). Optional.")]
    [ValidInterval]
    public string? DeepBackupInterval { get; init; }
}
