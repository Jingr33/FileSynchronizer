using FileSynchronizer.Configuration;
using Serilog;

namespace FileSynchronizer.Extensions.DependencyInjection;

public static class SerilogServiceCollectionExtensions
{
    private const string OutputTemplate = "[{Timestamp: yyyy-MM-dd HH:mm:ss, fff}] [{Level:u4}] [{SourceContext}] - {Message:lj}{NewLine}{Exception}";
    public static IHostBuilder ConfigureSerilog(this IHostBuilder builder, ApplicationOptions applicationOptions)
    {
        var logDirectory = Path.GetDirectoryName(applicationOptions.LogFile);
        if (!string.IsNullOrEmpty(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }

        return builder.UseSerilog(configureLogger: (context, _, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(
                    path: applicationOptions.LogFile,
                    fileSizeLimitBytes: 100000000,
                    rollingInterval: RollingInterval.Day,
                    rollOnFileSizeLimit: true,
                    outputTemplate: OutputTemplate
                );
        });
    }
}
